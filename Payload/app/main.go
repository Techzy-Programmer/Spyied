package main

import (
	"spyied_payload/activity"
)

func main() {
	activity.FireNetworkActivity()
	select {}
}
