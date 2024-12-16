package network

import (
	"encoding/json"
	"fmt"
	"io"
	"log"
	"net"
)

func NewTCPClient() *TCPClient {
	return &TCPClient{
		done: make(chan struct{}),
	}
}

func (c *TCPClient) Connect(ip string, port int) error {
	var err error
	c.conn, err = net.Dial("tcp", fmt.Sprintf("%s:%d", ip, port))
	if err != nil {
		return fmt.Errorf("failed to connect: %v", err)
	}

	c.stopRead = false
	c.wg.Add(1)
	go c.readLoop()

	return nil
}

func (c *TCPClient) SetMessageHandler(handler MSGHandler) {
	c.mu.Lock()
	defer c.mu.Unlock()
	c.handler = handler
}

func (c *TCPClient) Close() error {
	c.mu.Lock()
	if c.stopRead {
		c.mu.Unlock()
		return nil
	}
	c.stopRead = true
	c.mu.Unlock()

	close(c.done)
	c.wg.Wait()

	if c.conn != nil {
		return c.conn.Close()
	}
	return nil
}

func (c *TCPClient) SendMessage(msgType MSGType, payload interface{}) error {
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
	return err
}

// Continuously reads messages from the server
func (c *TCPClient) readLoop() {
	defer c.wg.Done()

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
			handler := c.handler
			c.mu.Unlock()

			if handler != nil {
				var payload interface{}
				switch msg.Type {
				case MSGTypeScreen:
					var chatMsg ScreenMSG
					if err := json.Unmarshal(msg.Payload, &chatMsg); err == nil {
						payload = chatMsg
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
