package network

import "net"

var udpConn *net.UDPConn

func InitUDPClient(ip string, port int) {
	var udpAddr = &net.UDPAddr{
		IP:   net.ParseIP(ip),
		Port: port,
	}

	var err error
	udpConn, err = net.DialUDP("udp", nil, udpAddr)
	if err != nil {
		panic(err)
	}
}

func SendUDPPacket(packet []byte) (int, error) {
	return udpConn.Write(packet)
}

func CloseUDPClient() {
	udpConn.Close()
	udpConn = nil
}
