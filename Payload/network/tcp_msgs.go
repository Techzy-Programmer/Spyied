package network

import (
	"encoding/json"
	"net"
	"sync"
)

type MSGType string

const (
	MSGTypeScreen MSGType = "screen"
)

type MSGHandler func(msgType MSGType, payload interface{})

// A TCP client instance
type TCPClient struct {
	conn     net.Conn
	handler  MSGHandler
	done     chan struct{}
	wg       sync.WaitGroup
	mu       sync.Mutex
	stopRead bool
}

type MSG struct {
	Type    MSGType         `json:"Type"`
	Payload json.RawMessage `json:"Payload"`
}

type ScreenMSG struct {
	ToPresent      bool   `json:"ToPresent"`
	DesiredQuality uint16 `json:"DesiredQuality"`
}
