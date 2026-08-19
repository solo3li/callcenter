import React, { useState, useEffect } from 'react';
import { StyleSheet, Text, View, TextInput, TouchableOpacity, SafeAreaView, ActivityIndicator } from 'react-native';
import * as signalR from '@microsoft/signalr';
import { Room, RoomEvent } from 'livekit-client';
import { LiveKitRoom, useRoomContext, VideoTrack } from '@livekit/react-native';

const BACKEND_URL = 'http://127.0.0.1:5000'; // Change to local IP if testing on physical device

export default function App() {
  const [username, setUsername] = useState('admin');
  const [password, setPassword] = useState('adminpassword');
  const [isLoggedIn, setIsLoggedIn] = useState(false);
  const [hubConnection, setHubConnection] = useState(null);
  
  const [incomingRoom, setIncomingRoom] = useState(null);
  const [activeToken, setActiveToken] = useState(null);
  const [livekitUrl, setLivekitUrl] = useState(null);

  const handleLogin = async () => {
    try {
      const response = await fetch(`${BACKEND_URL}/api/agent/login`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ username, password })
      });
      if (response.ok) {
        setIsLoggedIn(true);
        setupSignalR(username);
      } else {
        alert('Login failed');
      }
    } catch (e) {
      alert('Network error. Make sure backend is running.');
    }
  };

  const setupSignalR = (user) => {
    const connection = new signalR.HubConnectionBuilder()
      .withUrl(`${BACKEND_URL}/hubs/call?username=${user}`)
      .withAutomaticReconnect()
      .build();

    connection.on('IncomingTransfer', (roomName) => {
      console.log('Incoming call to room:', roomName);
      setIncomingRoom(roomName);
    });

    connection.start().catch(err => console.error('SignalR error:', err));
    setHubConnection(connection);
  };

  const answerCall = async () => {
    // Fetch token for this specific room
    try {
      const response = await fetch(`${BACKEND_URL}/api/token?identity=agent_${username}&room=${incomingRoom}`);
      const data = await response.json();
      setLivekitUrl(data.url);
      setActiveToken(data.token);
    } catch (e) {
      console.error(e);
    }
  };

  const endCall = () => {
    setActiveToken(null);
    setIncomingRoom(null);
  };

  if (activeToken) {
    return (
      <SafeAreaView style={styles.container}>
        <LiveKitRoom serverUrl={livekitUrl} token={activeToken} connect={true} audio={true}>
          <ActiveCallView onEndCall={endCall} roomName={incomingRoom} />
        </LiveKitRoom>
      </SafeAreaView>
    );
  }

  if (isLoggedIn) {
    return (
      <SafeAreaView style={styles.container}>
        <Text style={styles.title}>Agent Dashboard</Text>
        <Text>Status: Online</Text>
        
        {incomingRoom ? (
          <View style={styles.callBox}>
            <Text style={styles.ringingText}>Incoming Call!</Text>
            <Text>Room: {incomingRoom}</Text>
            <TouchableOpacity style={styles.answerBtn} onPress={answerCall}>
              <Text style={styles.btnText}>Answer</Text>
            </TouchableOpacity>
          </View>
        ) : (
          <View style={styles.idleBox}>
            <ActivityIndicator size="large" color="#0000ff" />
            <Text style={{marginTop: 10}}>Waiting for calls...</Text>
          </View>
        )}
      </SafeAreaView>
    );
  }

  return (
    <SafeAreaView style={styles.container}>
      <Text style={styles.title}>Agent Login</Text>
      <TextInput style={styles.input} value={username} onChangeText={setUsername} placeholder="Username" />
      <TextInput style={styles.input} value={password} onChangeText={setPassword} placeholder="Password" secureTextEntry />
      <TouchableOpacity style={styles.btn} onPress={handleLogin}>
        <Text style={styles.btnText}>Login</Text>
      </TouchableOpacity>
    </SafeAreaView>
  );
}

function ActiveCallView({ onEndCall, roomName }) {
  const room = useRoomContext();
  
  return (
    <View style={{flex: 1, justifyContent: 'center', alignItems: 'center'}}>
      <Text style={styles.title}>Active Call</Text>
      <Text>Connected to: {roomName}</Text>
      <TouchableOpacity style={[styles.btn, {backgroundColor: 'red', marginTop: 20}]} onPress={onEndCall}>
        <Text style={styles.btnText}>End Call</Text>
      </TouchableOpacity>
    </View>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1, padding: 20, justifyContent: 'center', alignItems: 'center' },
  title: { fontSize: 24, fontWeight: 'bold', marginBottom: 20 },
  input: { width: '80%', height: 40, borderColor: 'gray', borderWidth: 1, marginBottom: 15, paddingHorizontal: 10, borderRadius: 5 },
  btn: { backgroundColor: '#007AFF', padding: 10, borderRadius: 5, width: '80%', alignItems: 'center' },
  btnText: { color: 'white', fontWeight: 'bold' },
  callBox: { marginTop: 40, padding: 20, backgroundColor: '#FFE4E1', borderRadius: 10, alignItems: 'center' },
  ringingText: { fontSize: 20, color: 'red', fontWeight: 'bold', marginBottom: 10 },
  answerBtn: { backgroundColor: '#4CAF50', padding: 15, borderRadius: 10, marginTop: 15 },
  idleBox: { marginTop: 40, alignItems: 'center' }
});
