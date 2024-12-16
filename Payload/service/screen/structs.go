package screen

import (
	"image"
)

type Config struct {
	TargetFPS      int
	DisplayIndex   int
	NetworkEnabled bool
	BlockSize      int // Size of blocks to check for changes
}

type Capturer struct {
	config    *Config
	prevFrame []byte
	seqNum    uint16
	stopChan  chan struct{}
	bounds    image.Rectangle
}

type frame struct {
	sequenceNumber uint16
	data           []byte
	bounds         image.Rectangle
	dirtyRect      dirtyRect
	compressedData []byte
}

type dirtyRect struct {
	x      int32
	y      int32
	width  int32
	height int32
}

type immediateHeader struct {
	mType uint16
	uid   uint32
	ts    uint64
	sz    uint32
	x     uint32
	y     uint32
	w     uint32
	h     uint32
	q     uint16
}
