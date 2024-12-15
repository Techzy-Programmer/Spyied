package screen

import (
	"bytes"
	"compress/zlib"
	"encoding/binary"
	"math/rand"
	"time"
)

const HEADER_SIZE = 64

func splitIntoPackets(frame *frame) [][]byte {
	maxPayloadSize := MAX_PACKET_SIZE - HEADER_SIZE
	var packets [][]byte

	var iHeader = immediateHeader{
		mType: 1,
		uid:   uint32(rand.Int31()),
		ts:    uint64(time.Now().UnixMilli()),
		sz:    uint32(len(frame.compressedData)),
		x:     uint32(frame.dirtyRect.x),
		y:     uint32(frame.dirtyRect.y),
		w:     uint32(frame.dirtyRect.width),
		h:     uint32(frame.dirtyRect.height),
		q:     0,
	}

	if len(frame.compressedData) <= maxPayloadSize {
		packet := make([]byte, HEADER_SIZE+len(frame.compressedData))
		headerBytes := serializeHeader(1, 0, iHeader)

		copy(packet, headerBytes)
		copy(packet[HEADER_SIZE:], frame.compressedData)
		return [][]byte{packet}
	}

	numPackets := (len(frame.compressedData) + maxPayloadSize - 1) / maxPayloadSize
	for i := 0; i < numPackets; i++ {
		start := i * maxPayloadSize
		end := min(start+maxPayloadSize, len(frame.compressedData))
		var headerBytes = serializeHeader(uint16(numPackets), uint16(i), iHeader)

		packet := make([]byte, HEADER_SIZE+end-start)
		copy(packet, headerBytes)
		copy(packet[HEADER_SIZE:], frame.compressedData[start:end])
		packets = append(packets, packet)
	}

	return packets
}

func serializeHeader(count, seq uint16, ih immediateHeader) []byte {
	headerBytes := make([]byte, HEADER_SIZE)

	binary.LittleEndian.PutUint16(headerBytes[0:], ih.mType)
	binary.LittleEndian.PutUint32(headerBytes[2:], ih.uid)
	binary.LittleEndian.PutUint64(headerBytes[6:], ih.ts)
	binary.LittleEndian.PutUint16(headerBytes[14:], count)
	binary.LittleEndian.PutUint16(headerBytes[16:], seq)
	binary.LittleEndian.PutUint32(headerBytes[18:], ih.sz)
	binary.LittleEndian.PutUint32(headerBytes[22:], ih.x)
	binary.LittleEndian.PutUint32(headerBytes[26:], ih.y)
	binary.LittleEndian.PutUint32(headerBytes[30:], ih.w)
	binary.LittleEndian.PutUint32(headerBytes[34:], ih.h)
	binary.LittleEndian.PutUint16(headerBytes[38:], ih.q)

	return headerBytes
}

func compressData(data []byte) []byte {
	var buf bytes.Buffer
	w := zlib.NewWriter(&buf)
	w.Write(data)
	w.Close()
	return buf.Bytes()
}

func min(a, b int) int {
	if a < b {
		return a
	}
	return b
}

func max(a, b int) int {
	if a > b {
		return a
	}
	return b
}
