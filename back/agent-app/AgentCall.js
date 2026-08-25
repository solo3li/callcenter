import React, { useState, useEffect } from 'react';
import { Text, View, TouchableOpacity } from 'react-native';
import { LiveKitRoom, useLocalParticipant } from '@livekit/react-native';

export default function ActiveCall({ livekitUrl, token, roomName, handoffContext, muted, onToggleMute, onEndCall }) {
  return (
    <LiveKitRoom serverUrl={livekitUrl} token={token} connect={true} audio={true} video={false}>
      <ActiveCallView
        roomName={roomName}
        handoffContext={handoffContext}
        muted={muted}
        onToggleMute={onToggleMute}
        onEndCall={onEndCall}
      />
    </LiveKitRoom>
  );
}

function ActiveCallView({ roomName, handoffContext, muted, onToggleMute, onEndCall }) {
  const [elapsed, setElapsed] = useState(0);
  const { localParticipant } = useLocalParticipant();

  useEffect(() => {
    const timer = setInterval(() => setElapsed(s => s + 1), 1000);
    return () => clearInterval(timer);
  }, []);

  useEffect(() => {
    if (localParticipant) {
      localParticipant.setMicrophoneEnabled(!muted);
    }
  }, [muted, localParticipant]);

  const minutes = Math.floor(elapsed / 60);
  const seconds = elapsed % 60;

  return (
    <View style={{flex: 1, justifyContent: 'center', alignItems: 'center', padding: 20}}>
      <Text style={styles.title}>Active Call</Text>
      <Text>Connected to: {roomName}</Text>
      <Text style={{fontSize: 36, fontWeight: 'bold', marginVertical: 10, color: '#333'}}>
        {String(minutes).padStart(2, '0')}:{String(seconds).padStart(2, '0')}
      </Text>
      {handoffContext && (
        <View style={styles.contextBox}>
          <Text style={styles.contextTitle}>AI Handoff:</Text>
          <Text style={{fontSize: 13}}>{handoffContext.summary || JSON.stringify(handoffContext)}</Text>
        </View>
      )}
      <View style={[styles.actionRow, {marginTop: 15}]}>
        <TouchableOpacity
          style={[styles.btn, {flex: 1, marginRight: 5, backgroundColor: muted ? '#F44336' : '#2196F3'}]}
          onPress={onToggleMute}>
          <Text style={styles.btnText}>{muted ? '🔇 Muted' : '🎤 Mic On'}</Text>
        </TouchableOpacity>
        <TouchableOpacity style={[styles.btn, {flex: 1, marginLeft: 5, backgroundColor: 'red'}]} onPress={onEndCall}>
          <Text style={styles.btnText}>End Call</Text>
        </TouchableOpacity>
      </View>
    </View>
  );
}

const styles = {
  title: { fontSize: 24, fontWeight: 'bold', marginBottom: 5 },
  btn: { padding: 12, borderRadius: 8, alignItems: 'center', minWidth: 100 },
  btnText: { color: 'white', fontWeight: 'bold', fontSize: 14 },
  actionRow: { flexDirection: 'row', width: '90%' },
  contextBox: { marginTop: 10, padding: 12, backgroundColor: '#E3F2FD', borderRadius: 8, width: '90%', borderLeftWidth: 3, borderLeftColor: '#2196F3' },
  contextTitle: { fontWeight: 'bold', marginBottom: 5, color: '#1565C0' },
};
