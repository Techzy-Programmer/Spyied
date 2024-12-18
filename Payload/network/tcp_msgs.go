package network

import (
	"encoding/json"
	"net"
	"sync"
	"time"
)

type MSGType string

const (
	MSGTypeScreen MSGType = "screen"
)

type MSGHandler func(msgType MSGType, payload interface{})

type ConnectionConfig struct {
	IP          string
	Port        int
	RetryDelay  time.Duration // Delay between reconnection attempts
	MaxAttempts int           // Max number of attempts per reconnection cycle, -1 for infinite
}

// A TCP client instance
type TCPClient struct {
	conn             net.Conn
	msgHandler       MSGHandler
	connStateHandler func(isConnected bool)
	done             chan struct{}
	wg               sync.WaitGroup
	mu               sync.Mutex
	stopRead         bool

	// New fields for connection management
	config      *ConnectionConfig
	isConnected bool
	reconnectCh chan struct{}
}

type MSG struct {
	Type    MSGType         `json:"Type"`
	Payload json.RawMessage `json:"Payload"`
}

type ScreenMSG struct {
	ToPresent      bool   `json:"ToPresent"`
	DesiredQuality uint16 `json:"DesiredQuality"`
}
