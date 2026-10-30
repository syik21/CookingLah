
let api = null;
let chefParticipantId = null;
let chefSearchTimer = null;
let resizeObserver = null;
let sessionClosed = false;

window.addEventListener("cookinglah:chef-presence", function (event) {
    if (sessionClosed) return;

    const message = document.getElementById("chefPausedMessage");

    if (event.detail.present) {
        if (message) {
            message.hidden = true;
        }
        return;
    }

    showChefPausedMessage();
});

function showChefPausedMessage() {
    const container = document.getElementById("jitsi-container");
    let message = document.getElementById("chefPausedMessage");

    if (!container) return;

    if (!message) {
        message = document.createElement("div");
        message.id = "chefPausedMessage";
        message.className = "chef-paused-message";
        message.setAttribute("role", "status");
        message.innerHTML = "<h2>Stream paused</h2><p>The Chef paused the stream for a while.</p>";
        container.appendChild(message);
    }

    message.hidden = false;
}

window.addEventListener("cookinglah:live-ended", function () {
    sessionClosed = true;
    console.log("Chef ended the live class.");
    clearTimeout(chefSearchTimer);

    if (api) {
        api.dispose();
        api = null;
    }

    if (resizeObserver) {
        resizeObserver.disconnect();
        resizeObserver = null;
    }

    const container = document.getElementById("jitsi-container");
    const endedMessage = document.createElement("div");
    const title = document.createElement("h2");
    const detail = document.createElement("p");

    endedMessage.className = "live-ended-message";
    endedMessage.setAttribute("role", "status");
    title.textContent = "This live has ended";
    detail.textContent = "Thank you for joining the class.";
    endedMessage.append(title, detail);
    container.replaceChildren(endedMessage);

    document.getElementById("txtChatMessage").disabled = true;
    document.getElementById("btnSendMessage").disabled = true;
    document.getElementById("liveParticipantCount").textContent = "Live ended";

    $.connection.hub.stop();
});

window.addEventListener("cookinglah:live-suspended", function (event) {
    sessionClosed = true;
    clearTimeout(chefSearchTimer);
    if (api) {
        api.dispose();
        api = null;
    }
    if (resizeObserver) {
        resizeObserver.disconnect();
        resizeObserver = null;
    }

    const container = document.getElementById("jitsi-container");
    const message = document.createElement("div");
    message.className = "live-ended-message";
    message.innerHTML = "<h2>Livestream suspended</h2><p></p>";
    message.querySelector("p").textContent = event.detail.reason;
    container.replaceChildren(message);
    document.getElementById("txtChatMessage").disabled = true;
    document.getElementById("btnSendMessage").disabled = true;
    document.getElementById("liveParticipantCount").textContent = "Suspended";
    $.connection.hub.stop();
});

function loadJitsi() {

    const sessionId =
        document.getElementById("hfLiveSessionID")?.value;

    const roomName =
        document.getElementById("hfJitsiRoomName")?.value;

    const chefDisplayName =
        document.getElementById("hfChefDisplayName")?.value;

    const container =
        document.getElementById("jitsi-container");

    if (!sessionId || sessionId === "0" || !roomName || !chefDisplayName || !container) {
        console.error("Invalid session ID or Jitsi container.");
        return;
    }

    if (!window.jitsiDomain ||
        typeof JitsiMeetExternalAPI === "undefined") {
        console.error("Jitsi API is unavailable.");
        return;
    }

    // Prevent duplicate meetings.
    if (api) return;

    const options = {
        roomName: roomName,
        width: "100%",
        height: "100%",
        parentNode: container,

        userInfo: {
            displayName: "Learner"
        },

        configOverwrite: {
            prejoinConfig: {
                enabled: false
            },

            startWithAudioMuted: true,
            startWithVideoMuted: true,
            disableSelfView: true,
            doNotFlipLocalVideo: true,
            hideConferenceSubject: true,
            hideConferenceTimer: true,
            hideParticipantsStats: true,
            hideDisplayName: true,
            hideDominantSpeakerBadge: true,

            filmstrip: {
                disabled: true
            },

            tileView: {
                disabled: true
            },
            conferenceInfo: {
                alwaysVisible: [],
                autoHide: []
            },

            notifications: [],
            toolbarButtons: []
        },
        interfaceConfigOverwrite: {
            SHOW_JITSI_WATERMARK: false,
            SHOW_BRAND_WATERMARK: false,
            SHOW_POWERED_BY: false
        }
    };

    api = new JitsiMeetExternalAPI(
        window.jitsiDomain,
        options
    );

    function resizeJitsi() {
        if (api && container.clientWidth && container.clientHeight) {
            api.resizeLargeVideo(container.clientWidth, container.clientHeight);
        }
    }

    function updateParticipantCount() {
        const count = Math.max(api.getNumberOfParticipants() - 1, 0);
        document.getElementById("liveParticipantCount").textContent =
            count + (count === 1 ? " person" : " people") + " in this live";
    }

    resizeObserver = new ResizeObserver(resizeJitsi);
    resizeObserver.observe(container);

    // Find and pin the Chef only when necessary.
    async function showChefVideo(attemptsRemaining) {

        // Already pinned. Do not change the video again.
        if (!api || chefParticipantId) return;

        try {
            const rooms = await api.getRoomsInfo();

            const mainRoom = rooms.find(
                room => room.isMainRoom
            );

            if (!mainRoom || !mainRoom.participants) {
                scheduleChefSearch(attemptsRemaining);
                return;
            }

            const chef = mainRoom.participants.find(
                participant => participant.displayName === chefDisplayName
            );

            if (!chef) {
                console.log("Chef has not joined yet.");
                scheduleChefSearch(attemptsRemaining);
                return;
            }

            // Save the Chef's Jitsi participant ID.
            chefParticipantId = chef.id;
            clearTimeout(chefSearchTimer);

            // Disable tile view.
            api.executeCommand("setTileView", false);

            // Pin the Chef's camera.
            api.pinParticipant(chefParticipantId, "camera");

            // Select Chef as the large video once.
            api.setLargeVideoParticipant(chefParticipantId);

            resizeJitsi();

            console.log("Chef pinned:", chefParticipantId);

        } catch (error) {
            console.error("Unable to pin Chef:", error);
            scheduleChefSearch(attemptsRemaining);
        }
    }

    function scheduleChefSearch(attemptsRemaining) {
        clearTimeout(chefSearchTimer);

        if (attemptsRemaining <= 0) {
            return;
        }

        chefSearchTimer = setTimeout(function () {
            showChefVideo(attemptsRemaining - 1);
        }, 500);
    }

    // When learner joins the meeting.
    api.addListener("videoConferenceJoined", function () {

        console.log("Learner joined Jitsi.");

        requestAnimationFrame(resizeJitsi);
        updateParticipantCount();
        showChefVideo(10);
    });

    // When another participant joins.
    api.addListener("participantJoined", function (event) {

        console.log("Participant joined:", event.id);
        updateParticipantCount();

        // Only search if the Chef has not been pinned.
        if (!chefParticipantId) {
            showChefVideo(10);
        }
    });

    // When a participant leaves.
    api.addListener("participantLeft", function (event) {

        console.log("Participant left:", event.id);
        updateParticipantCount();

        // Only update if the Chef leaves.
        if (event.id === chefParticipantId) {

            chefParticipantId = null;
            clearTimeout(chefSearchTimer);

            console.log("Chef left the meeting.");

            // Do not immediately switch to another learner.
            // Wait for the Chef to rejoin.
        }
    });

    api.addListener("errorOccurred", function (error) {
        console.error("Jitsi error:", error);
    });

    api.addListener("videoConferenceLeft", function () {
        console.log("Learner left the meeting.");
    });
}

// Reload only when manually requested.
function reloadJitsi() {

    clearTimeout(chefSearchTimer);

    if (api) {
        api.dispose();
        api = null;
    }

    if (resizeObserver) {
        resizeObserver.disconnect();
        resizeObserver = null;
    }

    chefParticipantId = null;

    loadJitsi();
}

function exitLearnerLive(button) {
    if (!window.confirm("Are you sure you want to exit this live session?")) {
        return false;
    }

    clearTimeout(chefSearchTimer);

    if (resizeObserver) {
        resizeObserver.disconnect();
        resizeObserver = null;
    }

    button.disabled = true;

    const finishExit = function () {
        __doPostBack(button.name, "");
    };

    leaveJitsi(function () {
        if (window.leaveLiveChat) {
            window.leaveLiveChat().always(finishExit);
        } else {
            finishExit();
        }
    });

    return false;
}

function leaveJitsi(done) {
    const currentApi = api;
    api = null;

    if (!currentApi) {
        done();
        return;
    }

    let completed = false;
    const finish = function () {
        if (completed) return;
        completed = true;
        currentApi.dispose();
        done();
    };

    currentApi.addListener("videoConferenceLeft", finish);
    currentApi.executeCommand("hangup");
    setTimeout(finish, 1500);
}

document.addEventListener("DOMContentLoaded", loadJitsi);

document.addEventListener("DOMContentLoaded", function () {
    const button = document.getElementById("btnToggleChat");
    const layout = document.querySelector(".live-layout");

    if (!button || !layout) return;

    button.addEventListener("click", function () {
        const chatIsVisible = button.getAttribute("aria-expanded") === "true";
        layout.classList.toggle("live-layout--chat-hidden", chatIsVisible);
        button.setAttribute("aria-expanded", String(!chatIsVisible));
        button.textContent = chatIsVisible ? "Show chat" : "Hide chat";

        if (api) {
            requestAnimationFrame(function () {
                const container = document.getElementById("jitsi-container");
                api.resizeLargeVideo(container.clientWidth, container.clientHeight);
            });
        }
    });
});
