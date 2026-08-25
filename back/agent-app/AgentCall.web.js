import React, { useState, useEffect, useRef } from 'react';
import { Text, View, TouchableOpacity } from 'react-native';
import { Room, RoomEvent } from 'livekit-client';

function resolveLiveKitUrl(url) {
  if (!url) return url;
  try {
    const u = new URL(url.replace(/^ws(s?):/, 'http$1:'));
    if (u.hostname === 'livekit' || u.hostname === '127.0.0.1') {
      u.hostname = window.location.hostname;
    }
    return url.replace(/^[a-z]+:\/\/[^/]+/, `${u.protocol.replace('http', 'ws')}//${u.host}`);
  } catch {
    return url;
  }
}

export default function ActiveCall({ livekitUrl, token, roomName, handoffContext, muted, onToggleMute, onEndCall }) {
  const [elapsed, setElapsed] = useState(0);
  const [status, setStatus] = useState('connecting');
  const roomRef = useRef(null);

  useEffect(() => {
    const timer = setInterval(() => setElapsed(s => s + 1), 1000);
    return () => clearInterval(timer);
  }, []);

  useEffect(() => {
    const room = new Room();
    roomRef.current = room;
    let cancelled = false;

    const attachRemoteAudio = () => {
      room.remoteParticipants.forEach(p => {
        p.audioTrackPublications.forEach(pub => {
          const track = pub.track;
          if (track && track.attachedElements.length === 0) {
            track.attach();
          }
        });
      });
    };

    room.on(RoomEvent.TrackSubscribed, attachRemoteAudio);
    room.on(RoomEvent.ParticipantConnected, attachRemoteAudio);
    room.on(RoomEvent.Connected, () => !cancelled && setStatus('connected'));
    room.on(RoomEvent.Disconnected, () => !cancelled && setStatus('disconnected'));

    const connect = async () => {
      try {
        await room.connect(resolveLiveKitUrl(livekitUrl), token);
        if (cancelled) return;
        setStatus('connected');
        attachRemoteAudio();
        await room.localParticipant.setMicrophoneEnabled(true);
      } catch (err) {
        console.error('LiveKit web connect failed:', err);
        if (!cancelled) setStatus('error');
      }
    };
    void connect();

    return () => {
      cancelled = true;
      try { room.disconnect(); } catch {}
      roomRef.current = null;
    };
  }, [livekitUrl, token]);

  useEffect(() => {
    const p = roomRef.current?.localParticipant;
    if (p) p.setMicrophoneEnabled(!muted).catch(() => {});
  }, [muted]);

  const minutes = Math.floor(elapsed / 60);
  const seconds = elapsed % 60;

  const statusColor =
    status === 'connected' ? '#4CAF50' :
    status === 'error' || status === 'disconnected' ? '#F44336' : '#FF9800';

  return (
    <View style={{flex: 1, justifyContent: 'center', alignItems: 'center', padding: 20}}>
      <Text style={styles.title}>Active Call</Text>
      <Text>Connected to: {roomName}</Text>
      <View style={{flexDirection: 'row', alignItems: 'center', marginTop: 6}}>
        <View style={{width: 8, height: 8, borderRadius: 4, backgroundColor: statusColor, marginRight: 6}} />
        <Text style={{fontSize: 12, color: '#888'}}>{status}</Text>
      </View>
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
      <Text style={{fontSize: 11, color: '#999', marginTop: 12}}>browser audio mode · grant mic access when prompted</Text>
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
