import React, { useState, useEffect, useRef } from 'react';
import { StyleSheet, Text, View, TextInput, TouchableOpacity, SafeAreaView, ActivityIndicator } from 'react-native';
import * as signalR from '@microsoft/signalr';
import { LiveKitRoom, useRoomContext } from '@livekit/react-native';

const BACKEND_URL = 'http://127.0.0.1:5000';

export default function App() {
  const [accessKey, setAccessKey] = useState('');
  const [agentId, setAgentId] = useState(null);
  const [isLoggedIn, setIsLoggedIn] = useState(false);
  const [hubConnection, setHubConnection] = useState(null);
  const [agentName, setAgentName] = useState('');
  const [agentStatus, setAgentStatus] = useState('Available');

  const [incomingRoom, setIncomingRoom] = useState(null);
  const [callSessionId, setCallSessionId] = useState(null);
  const [transferId, setTransferId] = useState(null);
  const [handoffId, setHandoffId] = useState(null);
  const [activeToken, setActiveToken] = useState(null);
  const [livekitUrl, setLivekitUrl] = useState(null);
  const [handoffContext, setHandoffContext] = useState(null);
  const [statusLoading, setStatusLoading] = useState(false);

  const connectionRef = useRef(null);

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
        alert('Login failed. Check your access key.');
      }
    } catch (e) {
      alert('Network error. Make sure backend is running.');
    }
  };

  const setupSignalR = (agentId) => {
    const connection = new signalR.HubConnectionBuilder()
      .withUrl(`${BACKEND_URL}/hubs/call`)
      .withAutomaticReconnect()
      .build();

    connection.on('IncomingTransfer', (data) => {
      if (data.toHumanAgentId !== agentId) return;
      console.log('Incoming transfer for me:', data);
      setCallSessionId(data.callSessionId);
      setTransferId(data.transferId);
      setHandoffId(data.handoffId);
      setIncomingRoom(data.roomName);
    });

    connection.onreconnecting(() => console.log('SignalR reconnecting...'));
    connection.onreconnected((connectionId) => {
      console.log('SignalR reconnected:', connectionId);
      if (connectionId) {
        connection.invoke('RegisterAgent', agentId).catch(err =>
          console.error('Re-register failed:', err));
      }
    });

    connection.start().then(() => {
      connection.invoke('RegisterAgent', agentId);
    }).catch(err => console.error('SignalR error:', err));

    setHubConnection(connection);
    connectionRef.current = connection;
  };

  const setAgentStatusRemote = async (status) => {
    setStatusLoading(true);
    try {
      await fetch(`${BACKEND_URL}/api/human-agents/${agentId}/status`, {
        method: 'PATCH',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ status })
      });
      setAgentStatus(status);
    } catch (e) {
      console.error('Status update failed:', e);
    } finally {
      setStatusLoading(false);
    }
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
      setAgentStatus('In Call');
    } catch (e) {
      console.error(e);
    }
  };

  const rejectCall = async () => {
    try {
      await fetch(`${BACKEND_URL}/api/calls/${callSessionId}/transfers/${transferId}/reject`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ humanAgentId: agentId })
      });
      clearIncoming();
    } catch (e) {
      console.error(e);
    }
  };

  const clearIncoming = () => {
    setIncomingRoom(null);
    setCallSessionId(null);
    setTransferId(null);
    setHandoffId(null);
  };

  const endCall = async () => {
    try {
      if (transferId) {
        await fetch(`${BACKEND_URL}/api/calls/${callSessionId}/transfers/${transferId}/complete`, {
          method: 'POST'
        });
      } else if (callSessionId) {
        await fetch(`${BACKEND_URL}/api/calls/${callSessionId}/end`, {
          method: 'POST'
        });
      }
    } catch (e) {
      console.error('Failed to end call on backend:', e);
    }

    setActiveToken(null);
    setIncomingRoom(null);
    setCallSessionId(null);
    setTransferId(null);
    setHandoffId(null);
    setHandoffContext(null);
    setAgentStatus('Available');
  };

  const logout = async () => {
    if (connectionRef.current) {
      await connectionRef.current.stop();
    }
    setHubConnection(null);
    setAgentId(null);
    setAgentName('');
    setIsLoggedIn(false);
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
          <ActiveCallView
            onEndCall={endCall}
            roomName={incomingRoom}
            handoffContext={handoffContext}
          />
        </LiveKitRoom>
      </SafeAreaView>
    );
  }

  if (isLoggedIn) {
    return (
      <SafeAreaView style={styles.container}>
        <Text style={styles.title}>Agent Dashboard</Text>
        <Text>Status: {agentStatus} - {agentName}</Text>

        <View style={styles.statusRow}>
          <TouchableOpacity
            style={[styles.statusBtn, agentStatus === 'Available' && styles.statusActive]}
            onPress={() => setAgentStatusRemote('Available')}
            disabled={statusLoading}>
            <Text style={styles.statusBtnText}>Available</Text>
          </TouchableOpacity>
          <TouchableOpacity
            style={[styles.statusBtn, styles.statusWarning, agentStatus === 'Break' && styles.statusActive]}
            onPress={() => setAgentStatusRemote('Break')}
            disabled={statusLoading}>
            <Text style={styles.statusBtnText}>Break</Text>
          </TouchableOpacity>
          <TouchableOpacity
            style={[styles.statusBtn, styles.statusDanger, agentStatus === 'NotReady' && styles.statusActive]}
            onPress={() => setAgentStatusRemote('NotReady')}
            disabled={statusLoading}>
            <Text style={styles.statusBtnText}>Not Ready</Text>
          </TouchableOpacity>
        </View>

        {incomingRoom ? (
          <View style={styles.callBox}>
            <Text style={styles.ringingText}>Incoming Call!</Text>
            <Text>Room: {incomingRoom}</Text>
            <Text>Session: {callSessionId}</Text>
            <View style={styles.actionRow}>
              <TouchableOpacity style={styles.answerBtn} onPress={answerCall}>
                <Text style={styles.btnText}>Answer</Text>
              </TouchableOpacity>
              <TouchableOpacity style={styles.rejectBtn} onPress={rejectCall}>
                <Text style={styles.btnText}>Reject</Text>
              </TouchableOpacity>
            </View>
          </View>
        ) : (
          <View style={styles.idleBox}>
            <ActivityIndicator size="large" color="#0000ff" />
            <Text style={{marginTop: 10}}>Waiting for calls...</Text>
          </View>
        )}

        <TouchableOpacity style={[styles.btn, {backgroundColor: '#666', marginTop: 30}]} onPress={logout}>
          <Text style={styles.btnText}>Logout</Text>
        </TouchableOpacity>
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
  const [elapsed, setElapsed] = useState(0);

  useEffect(() => {
    const timer = setInterval(() => setElapsed(s => s + 1), 1000);
    return () => clearInterval(timer);
  }, []);

  const minutes = Math.floor(elapsed / 60);
  const seconds = elapsed % 60;

  return (
    <View style={{flex: 1, justifyContent: 'center', alignItems: 'center'}}>
      <Text style={styles.title}>Active Call</Text>
      <Text>Connected to: {roomName}</Text>
      <Text style={{fontSize: 28, fontWeight: 'bold', marginVertical: 10, color: '#333'}}>
        {String(minutes).padStart(2, '0')}:{String(seconds).padStart(2, '0')}
      </Text>
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
  answerBtn: { backgroundColor: '#4CAF50', padding: 15, borderRadius: 10, marginTop: 15, flex: 1, marginRight: 5 },
  rejectBtn: { backgroundColor: '#F44336', padding: 15, borderRadius: 10, marginTop: 15, flex: 1, marginLeft: 5 },
  actionRow: { flexDirection: 'row', marginTop: 10 },
  idleBox: { marginTop: 40, alignItems: 'center' },
  contextBox: { marginTop: 10, padding: 10, backgroundColor: '#f0f0f0', borderRadius: 5, width: '90%' },
  contextTitle: { fontWeight: 'bold', marginBottom: 5 },
  statusRow: { flexDirection: 'row', marginVertical: 15, gap: 8 },
  statusBtn: { paddingHorizontal: 12, paddingVertical: 8, borderRadius: 6, backgroundColor: '#e0e0e0', minWidth: 80, alignItems: 'center' },
  statusBtnText: { fontSize: 12, fontWeight: '600', color: '#555' },
  statusActive: { backgroundColor: '#4CAF50', borderWidth: 2, borderColor: '#2E7D32' },
  statusWarning: {},
  statusDanger: {},
});