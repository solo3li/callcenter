import React, { useState, useEffect } from 'react';
import { StyleSheet, Text, View, TextInput, TouchableOpacity, SafeAreaView, ActivityIndicator } from 'react-native';
import * as signalR from '@microsoft/signalr';
import { Room, RoomEvent } from 'livekit-client';
import { LiveKitRoom, useRoomContext, VideoTrack } from '@livekit/react-native';

const BACKEND_URL = 'http://127.0.0.1:5000';

export default function App() {
  const [accessKey, setAccessKey] = useState('');
  const [agentId, setAgentId] = useState(null);
  const [isLoggedIn, setIsLoggedIn] = useState(false);
  const [hubConnection, setHubConnection] = useState(null);
  const [agentName, setAgentName] = useState('');

  const [incomingRoom, setIncomingRoom] = useState(null);
  const [callSessionId, setCallSessionId] = useState(null);
  const [transferId, setTransferId] = useState(null);
  const [handoffId, setHandoffId] = useState(null);
  const [activeToken, setActiveToken] = useState(null);
  const [livekitUrl, setLivekitUrl] = useState(null);
  const [handoffContext, setHandoffContext] = useState(null);

  const handleLogin = async () => {
    try {
      const response = await fetch(`${BACKEND_URL}/api/auth/agent-login`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ accessKey: accessKey })
      });
      if (response.ok) {
        const data = await response.json();
        setAgentId(data.agentId);
        setAgentName(data.name);
        setLivekitUrl(data.livekitUrl);
        setIsLoggedIn(true);
        setupSignalR(data.agentId);
      } else {
        alert('Login failed');
      }
    } catch (e) {
      alert('Network error. Make sure backend is running.');
    }
  };

  const setupSignalR = (agentId) => {
    const connection = new signalR.HubConnectionBuilder()
      .withUrl(`${BACKEND_URL}/hubs/call?agent_id=${agentId}`)
      .withAutomaticReconnect()
      .build();

    connection.on('IncomingTransfer', (data) => {
      console.log('Incoming transfer:', data);
      setCallSessionId(data.callSessionId);
      setTransferId(data.transferId);
      setHandoffId(data.handoffId);
      setIncomingRoom(data.roomName);
    });

    connection.start().then(() => {
      connection.invoke('RegisterAgent', agentId);
    }).catch(err => console.error('SignalR error:', err));
    setHubConnection(connection);
  };

  const answerCall = async () => {
    try {
      await fetch(`${BACKEND_URL}/api/calls/${callSessionId}/transfers/${transferId}/accept`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ humanAgentId: agentId })
      });

      const ctxResp = await fetch(`${BACKEND_URL}/api/calls/${callSessionId}/handoffs/${handoffId}`);
      const ctx = await ctxResp.json();
      if (ctx.summary) {
        setHandoffContext(ctx);
      }

      const response = await fetch(`${BACKEND_URL}/api/livekit/token`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          identity: `agent_${agentId}`,
          roomName: incomingRoom,
          canPublish: true,
          canSubscribe: true
        })
      });
      const data = await response.json();
      setActiveToken(data.token);
    } catch (e) {
      console.error(e);
    }
  };

  const endCall = () => {
    setActiveToken(null);
    setIncomingRoom(null);
    setCallSessionId(null);
    setTransferId(null);
    setHandoffId(null);
    setHandoffContext(null);
  };

  if (activeToken) {
    return (
      <SafeAreaView style={styles.container}>
        <LiveKitRoom serverUrl={livekitUrl} token={activeToken} connect={true} audio={true}>
          <ActiveCallView onEndCall={endCall} roomName={incomingRoom} handoffContext={handoffContext} />
        </LiveKitRoom>
      </SafeAreaView>
    );
  }

  if (isLoggedIn) {
    return (
      <SafeAreaView style={styles.container}>
        <Text style={styles.title}>Agent Dashboard</Text>
        <Text>Status: Online - {agentName}</Text>

        {incomingRoom ? (
          <View style={styles.callBox}>
            <Text style={styles.ringingText}>Incoming Call!</Text>
            <Text>Room: {incomingRoom}</Text>
            <Text>Session: {callSessionId}</Text>
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
      <TextInput style={styles.input} value={accessKey} onChangeText={setAccessKey} placeholder="Access Key" />
      <TouchableOpacity style={styles.btn} onPress={handleLogin}>
        <Text style={styles.btnText}>Login</Text>
      </TouchableOpacity>
    </SafeAreaView>
  );
}

function ActiveCallView({ onEndCall, roomName, handoffContext }) {
  const room = useRoomContext();

  return (
    <View style={{flex: 1, justifyContent: 'center', alignItems: 'center'}}>
      <Text style={styles.title}>Active Call</Text>
      <Text>Connected to: {roomName}</Text>
      {handoffContext && (
        <View style={styles.contextBox}>
          <Text style={styles.contextTitle}>Handoff Context:</Text>
          <Text>{handoffContext.summary || JSON.stringify(handoffContext)}</Text>
        </View>
      )}
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
  idleBox: { marginTop: 40, alignItems: 'center' },
  contextBox: { marginTop: 10, padding: 10, backgroundColor: '#f0f0f0', borderRadius: 5, width: '90%' },
  contextTitle: { fontWeight: 'bold', marginBottom: 5 }
});