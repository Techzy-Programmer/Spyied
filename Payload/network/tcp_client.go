package network

import (
	"encoding/json"
	"fmt"
	"io"
	"log"
	"net"
	"time"
)

func NewTCPClient(config *ConnectionConfig) *TCPClient {
	if config.RetryDelay == 0 {
		config.RetryDelay = 10 * time.Second // Default retry delay
	}

	if config.MaxAttempts == 0 {
		config.MaxAttempts = -1 // Default to infinite attempts
	}

	return &TCPClient{
		done:        make(chan struct{}),
		reconnectCh: make(chan struct{}),
		config:      config,
	}
}

func (c *TCPClient) StartConnecting() error {
	c.wg.Add(1)
	go c.connectionManager()
	time.Sleep(1 * time.Second) // Delay initial connection attempt
	c.triggerReconnect()        // initial connection attempt

	return nil
}

func (c *TCPClient) connectionManager() {
	defer c.wg.Done()

	for {
		select {
		case <-c.done:
			return
		case <-c.reconnectCh:
			c.performConnection()
		}
	}
}

func (c *TCPClient) performConnection() {
	attempts := 0

	for c.config.MaxAttempts < 0 || attempts < c.config.MaxAttempts {
		select {
		case <-c.done:
			return
		default:
			if err := c.connect(); err != nil {
				log.Printf("Connection attempt %d failed: %v", attempts+1, err)
				attempts++
				time.Sleep(c.config.RetryDelay)
				continue
			}

			// Successfully connected
			log.Printf("Successfully connected to %s:%d", c.config.IP, c.config.Port)
			return
		}
	}
}

func (c *TCPClient) connect() error {
	c.mu.Lock()
	defer c.mu.Unlock()

	if c.conn != nil {
		c.conn.Close()
		c.conn = nil
	}

	var err error
	c.conn, err = net.Dial("tcp", fmt.Sprintf("%s:%d", c.config.IP, c.config.Port))
	if err != nil {
		c.isConnected = false
		return fmt.Errorf("failed to connect: %v", err)
	}

	c.connStateHandler(true)
	c.isConnected = true
	c.stopRead = false

	c.wg.Add(1)
	go c.readLoop()

	return nil
}

func (c *TCPClient) triggerReconnect() {
	select {
	case c.reconnectCh <- struct{}{}:
	default:
		// Channel is full, reconnection already pending
	}
}

func (c *TCPClient) handleDisconnect() {
	c.mu.Lock()
	wasConnected := c.isConnected
	c.isConnected = false
	c.connStateHandler(false)
	c.mu.Unlock()

	if wasConnected {
		log.Printf("Disconnected from %s:%d, attempting reconnection...", c.config.IP, c.config.Port)
		c.triggerReconnect()
	}
}

func (c *TCPClient) SetHandlers(msgHandler MSGHandler, connStateHandler func(isConnected bool)) {
	c.mu.Lock()
	defer c.mu.Unlock()
	c.msgHandler = msgHandler
	c.connStateHandler = connStateHandler
}

func (c *TCPClient) Close() error {
	c.mu.Lock()
	if c.stopRead {
		c.mu.Unlock()
		return nil
	}
	c.stopRead = true
	c.isConnected = false
	c.mu.Unlock()

	close(c.done)
	c.wg.Wait()

	if c.conn != nil {
		return c.conn.Close()
	}
	return nil
}

func (c *TCPClient) SendMessage(msgType MSGType, payload interface{}) error {
	c.mu.Lock()
	if !c.isConnected {
		c.mu.Unlock()
		return fmt.Errorf("client is not connected")
	}
	c.mu.Unlock()

	payloadBytes, err := json.Marshal(payload)
	if err != nil {
		return fmt.Errorf("failed to marshal payload: %v", err)
	}

	msg := MSG{
		Type:    msgType,
		Payload: payloadBytes,
	}

	msgBytes, err := json.Marshal(msg)
	if err != nil {
		return fmt.Errorf("failed to marshal message: %v", err)
	}

	// Add message length prefix
	length := uint32(len(msgBytes))
	lengthBytes := make([]byte, 4)
	lengthBytes[0] = byte(length >> 24)
	lengthBytes[1] = byte(length >> 16)
	lengthBytes[2] = byte(length >> 8)
	lengthBytes[3] = byte(length)

	// Send length prefix and message
	_, err = c.conn.Write(append(lengthBytes, msgBytes...))
	if err != nil {
		// Trigger reconnection on write error
		c.handleDisconnect()
		return err
	}

	return nil
}

// Continuously reads messages from the server
func (c *TCPClient) readLoop() {
	defer c.wg.Done()
	defer c.handleDisconnect()

	for {
		select {
		case <-c.done:
			return
		default:
			msg, err := c.readMessage()
			if err != nil {
				if err != io.EOF {
					log.Printf("Error reading message: %v", err)
				}
				return
			}

			c.mu.Lock()
			handler := c.msgHandler
			c.mu.Unlock()

			if handler != nil {
				var payload interface{}
				switch msg.Type {
				case MSGTypeScreen:
					var screenMsg ScreenMSG
					if err := json.Unmarshal(msg.Payload, &screenMsg); err == nil {
						payload = screenMsg
					}
				}

				if payload != nil {
					handler(msg.Type, payload)
				}
			}
		}
	}
}

func (c *TCPClient) readMessage() (*MSG, error) {
	lengthBytes := make([]byte, 4)
	_, err := io.ReadFull(c.conn, lengthBytes)
	if err != nil {
		return nil, err
	}

	length := uint32(lengthBytes[0])<<24 |
		uint32(lengthBytes[1])<<16 |
		uint32(lengthBytes[2])<<8 |
		uint32(lengthBytes[3])

	msgBytes := make([]byte, length)
	_, err = io.ReadFull(c.conn, msgBytes)
	if err != nil {
		return nil, err
	}

	var msg MSG
	err = json.Unmarshal(msgBytes, &msg)
	if err != nil {
		return nil, fmt.Errorf("failed to unmarshal message: %v", err)
	}

	return &msg, nil
}
