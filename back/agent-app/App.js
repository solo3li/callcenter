import React, { useState, useEffect, useRef, useCallback } from 'react';
import { StyleSheet, Text, View, TextInput, TouchableOpacity, SafeAreaView, ScrollView, Vibration, Platform, Modal } from 'react-native';
import * as signalR from '@microsoft/signalr';

// LiveKit's react-native SDK uses requireNativeComponent (native-only) — loading it
// unconditionally crashes web bundles. Metro resolves the unsuffixed specifier per
// platform: AgentCall.web.js for web, AgentCall.js for iOS/Android — never both.
const ActiveCall = require('./AgentCall').default;

const BACKEND_URL = Platform.select({
  ios: 'http://localhost:5000',
  android: 'http://10.0.2.2:5000',
  default: 'http://127.0.0.1:5000'
});

const TRANSFER_TIMEOUT_SECONDS = 30;

export default function App() {
  const [accessKey, setAccessKey] = useState('');
  const [agentId, setAgentId] = useState(null);
  const [ownerUserId, setOwnerUserId] = useState(null);
  const [isLoggedIn, setIsLoggedIn] = useState(false);
  const [hubConnection, setHubConnection] = useState(null);
  const [agentName, setAgentName] = useState('');
  const [agentStatus, setAgentStatus] = useState('Available');
  const [savedStatus, setSavedStatus] = useState('Available');
  const [isConnected, setIsConnected] = useState(false);

  const [incomingRoom, setIncomingRoom] = useState(null);
  const [callSessionId, setCallSessionId] = useState(null);
  const [transferId, setTransferId] = useState(null);
  const [handoffId, setHandoffId] = useState(null);
  const [activeToken, setActiveToken] = useState(null);
  const [livekitUrl, setLivekitUrl] = useState(null);
  const [handoffContext, setHandoffContext] = useState(null);
  const [statusLoading, setStatusLoading] = useState(false);
  const [activeScreen, setActiveScreen] = useState('idle');

  const [transferTimer, setTransferTimer] = useState(null);
  const [transferProgress, setTransferProgress] = useState(0);

  const [callHistory, setCallHistory] = useState([]);
  const [callNotes, setCallNotes] = useState('');
  const [showNotesScreen, setShowNotesScreen] = useState(false);
  const [lastCallInfo, setLastCallInfo] = useState(null);
  const [muted, setMuted] = useState(false);
  const [pendingTransfers, setPendingTransfers] = useState([]);
  const [transferSheetVisible, setTransferSheetVisible] = useState(false);
  const [transferOptions, setTransferOptions] = useState(null);
  const [transferring, setTransferring] = useState(false);

  const connectionRef = useRef(null);
  const ringIntervalRef = useRef(null);
  const timeoutRef = useRef(null);

  const clearRinging = useCallback(() => {
    Vibration.cancel();
    if (ringIntervalRef.current) clearInterval(ringIntervalRef.current);
    if (timeoutRef.current) clearTimeout(timeoutRef.current);
    setTransferProgress(0);
  }, []);

  const handleLogin = async () => {
    try {
      const response = await fetch(`${BACKEND_URL}/api/auth/agent-login`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ accessKey })
      });
      if (response.ok) {
        const data = await response.json();
        setAgentId(data.agentId);
        setAgentName(data.name);
        setOwnerUserId(data.ownerUserId);
        setLivekitUrl(data.livekitUrl);
        setIsLoggedIn(true);
        setupSignalR(data.agentId);
        fetchCallHistory(data.agentId);
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
      const transferData = { ...data, receivedAt: Date.now() };

      if (activeToken) {
        setPendingTransfers(prev => [...prev, transferData]);
        return;
      }

      setCallSessionId(data.callSessionId);
      setTransferId(data.transferId);
      setHandoffId(data.handoffId);
      setIncomingRoom(data.roomName);
      setActiveScreen('ringing');
      setTransferTimer(data.receivedAt);

      Vibration.vibrate([500, 300, 500]);
      ringIntervalRef.current = setInterval(() => {
        Vibration.vibrate(200);
      }, 3000);

      timeoutRef.current = setTimeout(() => {
        handleTransferTimeout();
      }, TRANSFER_TIMEOUT_SECONDS * 1000);
    });

    connection.on('TransferExpired', (data) => {
      if (data.transferId === transferId) {
        clearRinging();
        clearIncoming();
        setActiveScreen('idle');
      }
    });

    connection.onclose(() => setIsConnected(false));
    connection.onreconnecting(() => setIsConnected(false));
    connection.onreconnected((connectionId) => {
      setIsConnected(true);
      if (connectionId) {
        connection.invoke('RegisterAgent', agentId).catch(() => {});
      }
    });

    connection.start().then(() => {
      setIsConnected(true);
      connection.invoke('RegisterAgent', agentId);
    }).catch(err => {
      setIsConnected(false);
      console.error('SignalR error:', err);
    });

    setHubConnection(connection);
    connectionRef.current = connection;
  };

  const fetchCallHistory = async (agentId) => {
    try {
      const resp = await fetch(`${BACKEND_URL}/api/calls?limit=20`);
      if (resp.ok) {
        const data = await resp.json();
        setCallHistory(data.items || []);
      }
    } catch (e) {
      console.log('History fetch skipped (no auth):', e.message);
    }
  };

  const handleTransferTimeout = () => {
    clearRinging();
    clearIncoming();
    setActiveScreen('idle');
    alert('Call expired — you did not respond in time.');
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
      setSavedStatus(status);
    } catch (e) {
      console.error('Status update failed:', e);
    } finally {
      setStatusLoading(false);
    }
  };

  const answerCall = async () => {
    clearRinging();
    try {
      await fetch(`${BACKEND_URL}/api/call/transfer-decision`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ transferId, humanAgentId: agentId, decision: 'accept' })
      });

      try {
        const ctxResp = await fetch(`${BACKEND_URL}/api/calls/${callSessionId}/handoffs/${handoffId}`);
        const ctx = await ctxResp.json();
        if (ctx.summary) setHandoffContext(ctx);
      } catch (e) {
        console.log('No handoff context available');
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
      setSavedStatus(agentStatus);
      setAgentStatus('In Call');
      setActiveScreen('call');
    } catch (e) {
      console.error(e);
    }
  };

  const rejectCall = async () => {
    clearRinging();
    try {
      await fetch(`${BACKEND_URL}/api/call/transfer-decision`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ transferId, humanAgentId: agentId, decision: 'reject' })
      });
    } catch (e) {
      console.error(e);
    }
    clearIncoming();
    setActiveScreen('idle');
  };

  const clearIncoming = () => {
    setIncomingRoom(null);
    setCallSessionId(null);
    setTransferId(null);
    setHandoffId(null);
  };

  const openTransferSheet = async () => {
    setTransferSheetVisible(true);
    setTransferOptions(null);
    try {
      const resp = await fetch(
        `${BACKEND_URL}/api/call/transfer-options?roomName=${encodeURIComponent(incomingRoom || '')}&agentId=${agentId}`
      );
      if (resp.ok) setTransferOptions(await resp.json());
    } catch (e) {
      console.error('Failed to load transfer options:', e);
    }
  };

  const submitAgentTransfer = async (targetType, targetName) => {
    if (!targetName || transferring) return;
    setTransferring(true);
    try {
      const resp = await fetch(`${BACKEND_URL}/api/call/agent-transfer`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          roomName: incomingRoom,
          fromAgentId: agentId,
          targetType,
          targetName
        })
      });
      const body = await resp.json().catch(() => ({}));
      if (!resp.ok) {
        alert(body.error || 'Transfer failed');
      } else {
        setTransferSheetVisible(false);
        alert(`Transferring to ${body.targetName || targetName}…`);
      }
    } catch (e) {
      console.error('Agent transfer failed:', e);
      alert('Transfer failed');
    } finally {
      setTransferring(false);
    }
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

    setLastCallInfo({ callSessionId, transferId });
    setShowNotesScreen(true);

    setActiveToken(null);
    setIncomingRoom(null);
    setCallSessionId(null);
    setTransferId(null);
    setHandoffId(null);
    setHandoffContext(null);
    setMuted(false);
    setAgentStatus(savedStatus);
    setActiveScreen('idle');
  };

  const submitCallNotes = async () => {
    if (!callNotes.trim() || !lastCallInfo) {
      setShowNotesScreen(false);
      setCallNotes('');
      return;
    }
    try {
      if (lastCallInfo.callSessionId) {
        await fetch(`${BACKEND_URL}/api/calls/${lastCallInfo.callSessionId}/metadata`, {
          method: 'PATCH',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify({ metadataJson: JSON.stringify({ agentNotes: callNotes, disposition: 'completed' }) })
        });
      }
    } catch (e) {
      console.error('Failed to save notes:', e);
    }
    setShowNotesScreen(false);
    setCallNotes('');
    setLastCallInfo(null);
  };

  const logout = async () => {
    clearRinging();
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
    setIsConnected(false);
    setActiveScreen('idle');
    setCallHistory([]);
  };

  if (showNotesScreen) {
    return (
      <SafeAreaView style={styles.container}>
        <Text style={styles.title}>Call Notes</Text>
        <Text style={{marginBottom: 10, color: '#666'}}>Add disposition notes for call {lastCallInfo?.callSessionId?.slice(0, 8)}</Text>
        <TextInput
          style={[styles.input, { height: 120, textAlignVertical: 'top' }]}
          value={callNotes}
          onChangeText={setCallNotes}
          placeholder="What happened on this call?"
          multiline
        />
        <View style={styles.actionRow}>
          <TouchableOpacity style={styles.rejectBtn} onPress={() => { setShowNotesScreen(false); setCallNotes(''); }}>
            <Text style={styles.btnText}>Skip</Text>
          </TouchableOpacity>
          <TouchableOpacity style={styles.answerBtn} onPress={submitCallNotes}>
            <Text style={styles.btnText}>Save</Text>
          </TouchableOpacity>
        </View>
      </SafeAreaView>
    );
  }

  if (activeToken) {
    return (
      <SafeAreaView style={styles.container}>
        <ActiveCall
          livekitUrl={livekitUrl}
          token={activeToken}
          roomName={incomingRoom}
          handoffContext={handoffContext}
          muted={muted}
          onToggleMute={() => setMuted(m => !m)}
          onEndCall={endCall}
        />
        <TouchableOpacity style={styles.transferBtn} onPress={openTransferSheet}>
          <Text style={styles.transferBtnText}>⇄ Transfer call…</Text>
        </TouchableOpacity>

        <Modal
          visible={transferSheetVisible}
          transparent
          animationType="slide"
          onRequestClose={() => setTransferSheetVisible(false)}
        >
          <View style={styles.sheetBackdrop}>
            <View style={styles.sheetCard}>
              <Text style={styles.sheetTitle}>Transfer this call</Text>
              {!transferOptions ? (
                <Text style={styles.sheetHint}>loading options…</Text>
              ) : (
                <>
                  <ScrollView style={{ maxHeight: 180 }}>
                    {transferOptions.agents?.length > 0 && (
                      <>
                        <Text style={styles.sectionLabel}>Colleagues</Text>
                        {transferOptions.agents.map(a => (
                          <TouchableOpacity
                            key={`a-${a.id}`}
                            style={[styles.listItem, !a.available && styles.listItemDisabled]}
                            disabled={!a.available || transferring}
                            onPress={() => submitAgentTransfer('human', a.name)}
                          >
                            <Text style={styles.listText}>{a.available ? a.name : `${a.name} (busy)`}</Text>
                          </TouchableOpacity>
                        ))}
                      </>
                    )}
                    {transferOptions.destinations?.length > 0 && (
                      <>
                        <Text style={styles.sectionLabel}>External destinations</Text>
                        {transferOptions.destinations.map(d => (
                          <TouchableOpacity
                            key={`d-${d.id}`}
                            style={styles.listItem}
                            disabled={transferring}
                            onPress={() => submitAgentTransfer('destination', d.name)}
                          >
                            <Text style={styles.listText}>⇒ {d.name}</Text>
                          </TouchableOpacity>
                        ))}
                      </>
                    )}
                  </ScrollView>
                  {(transferOptions.agents?.length ?? 0) === 0 && (transferOptions.destinations?.length ?? 0) === 0 && (
                    <Text style={styles.sheetHint}>No transfer targets configured.</Text>
                  )}
                </>
              )}
              <TouchableOpacity style={styles.sheetClose} onPress={() => setTransferSheetVisible(false)}>
                <Text style={styles.btnText}>Cancel</Text>
              </TouchableOpacity>
            </View>
          </View>
        </Modal>
      </SafeAreaView>
    );
  }

  if (isLoggedIn) {
    return (
      <SafeAreaView style={styles.container}>
        <View style={styles.header}>
          <Text style={styles.title}>Agent Dashboard</Text>
          <View style={styles.headerRight}>
            <View style={[styles.connDot, isConnected ? styles.connGreen : styles.connRed]} />
            <Text style={styles.connText}>{isConnected ? 'Online' : 'Offline'}</Text>
          </View>
        </View>
        <Text style={styles.subtitle}>{agentName}</Text>

        <View style={styles.statusRow}>
          {['Available', 'Break', 'NotReady'].map(s => (
            <TouchableOpacity
              key={s}
              style={[
                styles.statusBtn,
                s === 'Available' && styles.statusAvail,
                s === 'Break' && styles.statusBreak,
                s === 'NotReady' && styles.statusNrdy,
                agentStatus === s && styles.statusActive
              ]}
              onPress={() => setAgentStatusRemote(s)}
              disabled={statusLoading || agentStatus === 'In Call'}>
              <Text style={[styles.statusBtnText, agentStatus === s && styles.statusActiveText]}>
                {s === 'NotReady' ? 'Not Ready' : s}
              </Text>
            </TouchableOpacity>
          ))}
        </View>

        <View style={styles.tabRow}>
          <TouchableOpacity
            style={[styles.tab, activeScreen === 'idle' && styles.tabActive]}
            onPress={() => { clearRinging(); clearIncoming(); setActiveScreen('idle'); }}>
            <Text style={[styles.tabText, activeScreen === 'idle' && styles.tabActiveText]}>Calls</Text>
            {pendingTransfers.length > 0 && (
              <View style={styles.badge}><Text style={styles.badgeText}>{pendingTransfers.length}</Text></View>
            )}
          </TouchableOpacity>
          <TouchableOpacity
            style={[styles.tab, activeScreen === 'history' && styles.tabActive]}
            onPress={() => setActiveScreen('history')}>
            <Text style={[styles.tabText, activeScreen === 'history' && styles.tabActiveText]}>History</Text>
          </TouchableOpacity>
        </View>

        {activeScreen === 'history' && (
          <ScrollView style={styles.historyList}>
            {callHistory.length === 0 ? (
              <Text style={styles.emptyText}>No calls yet</Text>
            ) : (
              callHistory.map((call, i) => (
                <View key={call.id || i} style={styles.historyItem}>
                  <View style={styles.historyRow}>
                    <Text style={styles.historyRoom}>{call.roomName || call.livekitRoomName || `Call #${i + 1}`}</Text>
                    <Text style={[
                      styles.historyStatus,
                      (call.status === 'Completed' || call.status === 'Transferred') && { color: '#4CAF50' },
                      call.status === 'Active' && { color: '#2196F3' }
                    ]}>{call.status || 'Unknown'}</Text>
                  </View>
                  <Text style={styles.historyDate}>{call.startedAt ? new Date(call.startedAt).toLocaleString() : 'N/A'}</Text>
                </View>
              ))
            )}
          </ScrollView>
        )}

        {activeScreen === 'idle' && incomingRoom && (
          <View style={styles.callBox}>
            <Text style={styles.ringingText}>🔔 Incoming Call!</Text>
            <Text>Room: {incomingRoom}</Text>
            <Text style={styles.timerText}>Expires in: {TRANSFER_TIMEOUT_SECONDS - transferProgress}s</Text>
            <View style={styles.actionRow}>
              <TouchableOpacity style={styles.answerBtn} onPress={answerCall}>
                <Text style={styles.btnText}>Answer</Text>
              </TouchableOpacity>
              <TouchableOpacity style={styles.rejectBtn} onPress={rejectCall}>
                <Text style={styles.btnText}>Reject</Text>
              </TouchableOpacity>
            </View>
          </View>
        )}

        {activeScreen === 'idle' && !incomingRoom && pendingTransfers.length > 0 && (
          <View style={styles.callBox}>
            <Text style={{fontSize: 16, fontWeight: '600', marginBottom: 5}}>
              You have {pendingTransfers.length} pending transfer{pendingTransfers.length > 1 ? 's' : ''}
            </Text>
            <Text style={{color: '#888', fontSize: 13}}>Complete current call first</Text>
          </View>
        )}

        {activeScreen === 'idle' && !incomingRoom && pendingTransfers.length === 0 && (
          <View style={styles.idleBox}>
            <View style={[styles.pulseCircle, agentStatus === 'Available' ? { backgroundColor: '#4CAF50' } : { backgroundColor: '#FF9800' }]} />
            <Text style={{marginTop: 15, fontSize: 16, color: '#666'}}>Waiting for calls...</Text>
            <Text style={{fontSize: 12, color: '#999', marginTop: 5}}>Status: {agentStatus}</Text>
          </View>
        )}

        <TouchableOpacity style={[styles.btn, {backgroundColor: '#666', marginTop: 20}]} onPress={logout}>
          <Text style={styles.btnText}>Logout</Text>
        </TouchableOpacity>
      </SafeAreaView>
    );
  }

  return (
    <SafeAreaView style={styles.container}>
      <Text style={styles.title}>Agent Login</Text>
      <TextInput style={styles.input} value={accessKey} onChangeText={setAccessKey} placeholder="Access Key" autoCapitalize="none" />
      <TouchableOpacity style={styles.btn} onPress={handleLogin}>
        <Text style={styles.btnText}>Login</Text>
      </TouchableOpacity>
      <Text style={{marginTop: 20, fontSize: 11, color: '#999'}}>Server: {BACKEND_URL}</Text>
    </SafeAreaView>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1, padding: 20, justifyContent: 'center', alignItems: 'center', backgroundColor: '#fafafa' },
  title: { fontSize: 24, fontWeight: 'bold', marginBottom: 5 },
  subtitle: { fontSize: 14, color: '#666', marginBottom: 15 },
  input: { width: '80%', height: 40, borderColor: 'gray', borderWidth: 1, marginBottom: 15, paddingHorizontal: 10, borderRadius: 5, backgroundColor: '#fff' },
  btn: { padding: 12, borderRadius: 8, alignItems: 'center', minWidth: 100 },
  btnText: { color: 'white', fontWeight: 'bold', fontSize: 14 },
  callBox: { marginTop: 20, padding: 20, backgroundColor: '#FFF3E0', borderRadius: 12, alignItems: 'center', width: '90%', borderWidth: 1, borderColor: '#FFE0B2' },
  ringingText: { fontSize: 20, color: '#E65100', fontWeight: 'bold', marginBottom: 10 },
  timerText: { fontSize: 13, color: '#FF5722', marginTop: 5, fontWeight: '500' },
  answerBtn: { backgroundColor: '#4CAF50', padding: 15, borderRadius: 10, flex: 1, marginRight: 5, alignItems: 'center' },
  rejectBtn: { backgroundColor: '#F44336', padding: 15, borderRadius: 10, flex: 1, marginLeft: 5, alignItems: 'center' },
  actionRow: { flexDirection: 'row', width: '90%' },
  idleBox: { marginTop: 60, alignItems: 'center' },
  contextBox: { marginTop: 10, padding: 12, backgroundColor: '#E3F2FD', borderRadius: 8, width: '90%', borderLeftWidth: 3, borderLeftColor: '#2196F3' },
  contextTitle: { fontWeight: 'bold', marginBottom: 5, color: '#1565C0' },
  statusRow: { flexDirection: 'row', marginVertical: 10, gap: 6 },
  statusBtn: { paddingHorizontal: 12, paddingVertical: 8, borderRadius: 8, minWidth: 75, alignItems: 'center' },
  statusAvail: { backgroundColor: '#E8F5E9' },
  statusBreak: { backgroundColor: '#FFF8E1' },
  statusNrdy: { backgroundColor: '#FFEBEE' },
  statusActive: { borderWidth: 2, borderColor: '#333' },
  statusBtnText: { fontSize: 12, fontWeight: '600', color: '#555' },
  statusActiveText: { color: '#111' },
  header: { flexDirection: 'row', alignItems: 'center', justifyContent: 'space-between', width: '90%' },
  headerRight: { flexDirection: 'row', alignItems: 'center', gap: 5 },
  connDot: { width: 8, height: 8, borderRadius: 4 },
  connGreen: { backgroundColor: '#4CAF50' },
  connRed: { backgroundColor: '#F44336' },
  connText: { fontSize: 12, color: '#888' },
  pulseCircle: { width: 60, height: 60, borderRadius: 30, opacity: 0.8 },
  tabRow: { flexDirection: 'row', width: '90%', marginVertical: 10, gap: 0 },
  tab: { flex: 1, paddingVertical: 10, alignItems: 'center', borderBottomWidth: 2, borderBottomColor: 'transparent', flexDirection: 'row', justifyContent: 'center' },
  tabActive: { borderBottomColor: '#2196F3' },
  tabText: { fontSize: 15, fontWeight: '500', color: '#888' },
  tabActiveText: { color: '#2196F3' },
  badge: { backgroundColor: '#F44336', borderRadius: 10, minWidth: 18, height: 18, alignItems: 'center', justifyContent: 'center', marginLeft: 6 },
  badgeText: { color: '#fff', fontSize: 11, fontWeight: 'bold', paddingHorizontal: 4 },
  historyList: { width: '90%', flex: 1, marginTop: 5 },
  historyItem: { padding: 12, borderBottomWidth: 1, borderBottomColor: '#eee', backgroundColor: '#fff', borderRadius: 6, marginBottom: 4 },
  historyRow: { flexDirection: 'row', justifyContent: 'space-between' },
  historyRoom: { fontWeight: '600', fontSize: 14 },
  historyStatus: { fontSize: 12, fontWeight: '500' },
  historyDate: { fontSize: 11, color: '#999', marginTop: 3 },
  emptyText: { textAlign: 'center', color: '#999', marginTop: 30, fontSize: 14 },
  transferBtn: {
    backgroundColor: '#2f6f4e',
    paddingVertical: 10,
    paddingHorizontal: 22,
    borderRadius: 8,
    marginTop: 12,
  },
  transferBtnText: { color: '#fff', fontWeight: '600', fontSize: 14 },
  sheetBackdrop: { flex: 1, backgroundColor: 'rgba(0,0,0,0.45)', justifyContent: 'flex-end' },
  sheetCard: {
    backgroundColor: '#fff',
    borderTopLeftRadius: 16,
    borderTopRightRadius: 16,
    padding: 18,
    maxHeight: '70%',
  },
  sheetTitle: { fontSize: 17, fontWeight: '700', marginBottom: 10 },
  sheetHint: { color: '#888', fontSize: 13, paddingVertical: 8 },
  sectionLabel: {
    fontSize: 11,
    textTransform: 'uppercase',
    letterSpacing: 1,
    color: '#999',
    marginTop: 8,
    marginBottom: 4,
  },
  listItem: { paddingVertical: 11, borderBottomWidth: 1, borderColor: '#eee' },
  listItemDisabled: { opacity: 0.45 },
  listText: { fontSize: 15 },
  sheetClose: {
    backgroundColor: '#666',
    alignItems: 'center',
    paddingVertical: 11,
    borderRadius: 8,
    marginTop: 14,
  },
});