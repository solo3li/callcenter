const btnConnect = document.getElementById('btn-connect');
const btnDisconnect = document.getElementById('btn-disconnect');
const statusText = document.getElementById('status-text');
const visualizer = document.getElementById('visualizer-container');
const mediaContainer = document.getElementById('media-container');

let room = null;

async function startCall() {
    try {
        updateUIState('connecting');

        // Fetch token from ASP.NET Core backend
        const response = await fetch('/api/token');
        if (!response.ok) throw new Error('Failed to fetch token');
        
        const data = await response.json();
        let { token, url } = data;
        
        // If the backend returns localhost, replace it with the actual VPS IP/hostname 
        // that the browser used to access the page.
        if (url.includes('127.0.0.1') || url.includes('localhost')) {
            url = `ws://${window.location.hostname}:7880`;
        }

        // Initialize LiveKit Room
        room = new LivekitClient.Room({
            adaptiveStream: true,
            dynacast: true,
        });

        // Setup event listeners
        room.on(LivekitClient.RoomEvent.TrackSubscribed, (track, publication, participant) => {
            if (track.kind === LivekitClient.Track.Kind.Audio) {
                const element = track.attach();
                mediaContainer.appendChild(element);
                
                // Animate UI to indicate AI is connected and can speak
                visualizer.classList.add('is-active');
            }
        });

        room.on(LivekitClient.RoomEvent.TrackUnsubscribed, (track) => {
            track.detach();
            visualizer.classList.remove('is-active');
        });

        room.on(LivekitClient.RoomEvent.Disconnected, () => {
            handleDisconnect();
        });

        // Connect to the room
        await room.connect(url, token);

        // Publish local microphone
        await room.localParticipant.setMicrophoneEnabled(true);
        
        updateUIState('connected');

    } catch (error) {
        console.error('Connection failed:', error);
        updateUIState('disconnected');
        alert('Failed to connect to the AI Assistant. See console for details.');
    }
}

async function endCall() {
    if (room) {
        await room.disconnect();
    }
    handleDisconnect();
}

function handleDisconnect() {
    room = null;
    visualizer.classList.remove('is-active');
    mediaContainer.innerHTML = '';
    updateUIState('disconnected');
}

function updateUIState(state) {
    switch(state) {
        case 'connecting':
            btnConnect.textContent = 'Connecting...';
            btnConnect.disabled = true;
            statusText.textContent = 'Connecting to AI...';
            statusText.style.color = 'var(--text-secondary)';
            break;
        case 'connected':
            btnConnect.classList.add('hidden');
            btnDisconnect.classList.remove('hidden');
            btnConnect.disabled = false;
            btnConnect.textContent = 'Start Call';
            statusText.textContent = 'Connected. Start speaking!';
            statusText.style.color = 'var(--primary)';
            break;
        case 'disconnected':
            btnDisconnect.classList.add('hidden');
            btnConnect.classList.remove('hidden');
            btnConnect.disabled = false;
            btnConnect.textContent = 'Start Call';
            statusText.textContent = 'Disconnected';
            statusText.style.color = 'var(--text-secondary)';
            break;
    }
}

// Event Listeners
btnConnect.addEventListener('click', startCall);
btnDisconnect.addEventListener('click', endCall);
