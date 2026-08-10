package main

import (
	"fmt"
	"log"
	"os"

	"github.com/livekit/server-sdk-go/v2"
	"github.com/pion/webrtc/v3"
)

func main() {
	apiKey := os.Getenv("LIVEKIT_API_KEY")
	apiSecret := os.Getenv("LIVEKIT_API_SECRET")
	livekitURL := os.Getenv("LIVEKIT_URL")
	geminiKey := os.Getenv("GEMINI_API_KEY")

	if apiKey == "" || apiSecret == "" || livekitURL == "" || geminiKey == "" {
		log.Fatal("Missing required environment variables")
	}

	fmt.Println("AI Worker started. Waiting for connections...")
	
	roomName := "sip-room"
	
	// Example of joining a room
	roomCB := &lksdk.RoomCallback{
		ParticipantCallback: lksdk.ParticipantCallback{
			OnTrackSubscribed: func(track *webrtc.TrackRemote, publication *lksdk.RemoteTrackPublication, rp *lksdk.RemoteParticipant) {
				fmt.Printf("Track subscribed: %s\n", track.ID())
				// Here we would pipe audio to Gemini API and stream responses back
			},
		},
	}
	
	room, err := lksdk.ConnectToRoom(livekitURL, lksdk.ConnectInfo{
		APIKey:              apiKey,
		APISecret:           apiSecret,
		RoomName:            roomName,
		ParticipantIdentity: "ai-agent",
	}, roomCB)
	
	if err != nil {
		log.Fatalf("Could not connect to room: %v", err)
	}
	
	defer room.Disconnect()
	
	// Wait forever
	select {}
}
