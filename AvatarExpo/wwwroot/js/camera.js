window.avatarCamera = {
    stream: null,
    video: null,
    canvas: null,
    ctx: null,
    skeletonCanvas: null,
    skeletonCtx: null,
    intervalId: null,
    onFrameCallback: null,
    fps: 30,
    quality: 0.6,
    isRunning: false,

    displayStream: null,
    displayVideo: null,

    start: async function (dotNetRef) {
        try {
            this.stream = await navigator.mediaDevices.getUserMedia({
                video: { width: 640, height: 480, frameRate: 30 },
                audio: false
            });

            this.video = document.getElementById('webcam-video');
            if (this.video) {
                this.video.srcObject = this.stream;
                this.video.style.display = 'block';
                await this.video.play();
            } else {
                console.error('Video element #webcam-video not found in DOM');
                return false;
            }

            this.canvas = document.createElement('canvas');
            this.canvas.width = 640;
            this.canvas.height = 480;
            this.ctx = this.canvas.getContext('2d');

            this.skeletonCanvas = document.getElementById('skeleton-canvas');
            if (this.skeletonCanvas) {
                this.skeletonCtx = this.skeletonCanvas.getContext('2d');
            }

            this.dotNetRef = dotNetRef;
            this.isRunning = true;
            this.startCaptureLoop();

            return true;
        } catch (err) {
            console.error('Camera error:', err);
            return false;
        }
    },

    stop: function () {
        this.isRunning = false;
        if (this.intervalId) {
            clearInterval(this.intervalId);
            this.intervalId = null;
        }
        if (this.skeletonCanvas && this.skeletonCtx) {
            this.skeletonCtx.clearRect(0, 0, 640, 480);
        }
        if (this.stream) {
            this.stream.getTracks().forEach(t => t.stop());
            this.stream = null;
        }
        if (this.video) {
            this.video.srcObject = null;
            this.video.style.display = 'none';
            this.video = null;
        }
        this.stopDisplayCapture();
    },

    startCaptureLoop: function () {
        const self = this;
        const intervalMs = 1000 / this.fps;

        this.intervalId = setInterval(function () {
            if (!self.isRunning || !self.video || self.video.readyState < 2) return;

            self.ctx.drawImage(self.video, 0, 0, 640, 480);

            self.canvas.toBlob(function (blob) {
                if (!blob) return;
                blob.arrayBuffer().then(function (buffer) {
                    const uint8 = new Uint8Array(buffer);
                    self.dotNetRef.invokeMethodAsync('ReceiveFrame', uint8);
                });
            }, 'image/jpeg', self.quality);
        }, intervalMs);
    },

    setFps: function (newFps) {
        this.fps = newFps;
        if (this.isRunning) {
            if (this.intervalId) clearInterval(this.intervalId);
            this.startCaptureLoop();
        }
    },

    setQuality: function (newQuality) {
        this.quality = newQuality;
    },

    drawSkeleton: function (skeletonData) {
        if (!this.skeletonCtx || !skeletonData || !skeletonData.landmarks) return;

        var ctx = this.skeletonCtx;
        var landmarks = skeletonData.landmarks;
        var connections = skeletonData.connections;
        var w = skeletonData.width || 640;
        var h = skeletonData.height || 480;

        ctx.clearRect(0, 0, 640, 480);

        if (connections) {
            for (var i = 0; i < connections.length; i++) {
                var conn = connections[i];
                var a = landmarks[conn[0]];
                var b = landmarks[conn[1]];
                if (!a || !b) continue;

                ctx.beginPath();
                ctx.moveTo(a[0] * w, a[1] * h);
                ctx.lineTo(b[0] * w, b[1] * h);
                ctx.strokeStyle = '#00ff00';
                ctx.lineWidth = 2;
                ctx.stroke();
            }
        }

        for (var j = 0; j < landmarks.length; j++) {
            var lm = landmarks[j];
            if (!lm) continue;
            ctx.beginPath();
            ctx.arc(lm[0] * w, lm[1] * h, 3, 0, 2 * Math.PI);
            ctx.fillStyle = '#00ff00';
            ctx.fill();
        }
    },

    initDisplayCapture: async function () {
        if (this.displayStream) return;

        try {
            this.displayStream = await navigator.mediaDevices.getDisplayMedia({
                video: { frameRate: 30 },
                preferCurrentTab: true
            });

            this.displayVideo = document.createElement('video');
            this.displayVideo.srcObject = this.displayStream;
            this.displayVideo.autoplay = true;
            this.displayVideo.playsInline = true;
            this.displayVideo.style.display = 'none';
            document.body.appendChild(this.displayVideo);

            await this.displayVideo.play();
        } catch (err) {
            console.warn('getDisplayMedia failed, will use html2canvas fallback:', err);
            this.displayStream = null;
            this.displayVideo = null;
        }
    },

    stopDisplayCapture: function () {
        if (this.displayStream) {
            this.displayStream.getTracks().forEach(function (t) { t.stop(); });
            this.displayStream = null;
        }
        if (this.displayVideo) {
            this.displayVideo.srcObject = null;
            if (this.displayVideo.parentNode) {
                this.displayVideo.parentNode.removeChild(this.displayVideo);
            }
            this.displayVideo = null;
        }
    },

    captureWithDisplayMedia: async function () {
        var container = document.querySelector('.avatar-container');
        if (!container) throw new Error('Avatar container not found');

        var scaleX = this.displayVideo.videoWidth / window.innerWidth;
        var scaleY = this.displayVideo.videoHeight / window.innerHeight;

        var r = container.getBoundingClientRect();
        var cropX = r.left * scaleX;
        var cropY = r.top * scaleY;
        var cropSrcW = r.width * scaleX;
        var cropSrcH = r.height * scaleY;

        var cropped = document.createElement('canvas');
        cropped.width = cropSrcW * 2;
        cropped.height = cropSrcH * 2;

        var croppedCtx = cropped.getContext('2d');
        croppedCtx.fillStyle = '#1a1a1a';
        croppedCtx.fillRect(0, 0, cropped.width, cropped.height);

        croppedCtx.drawImage(
            this.displayVideo,
            cropX, cropY, cropSrcW, cropSrcH,
            0, 0, cropped.width, cropped.height
        );

        return cropped;
    },

    captureWithHtml2canvas: async function () {
        var wrapper = document.getElementById('avatar-wrapper');
        if (!wrapper) throw new Error('Avatar wrapper not found');

        var rect = wrapper.getBoundingClientRect();
        var padding = 120;

        var canvas = await html2canvas(wrapper, {
            backgroundColor: '#1a1a1a',
            scale: 2,
            width: rect.width + padding * 2,
            height: rect.height + padding * 2,
            x: -padding,
            y: -padding,
            allowTaint: true,
            useCORS: true
        });

        return canvas;
    },

    applyWatermark: async function (canvas) {
        var watermarkImg = new Image();
        watermarkImg.src = 'imgs/et12.svg';

        await new Promise(function (resolve, reject) {
            watermarkImg.onload = resolve;
            watermarkImg.onerror = reject;
        });

        var ctx = canvas.getContext('2d');
        var wmWidth = Math.min(canvas.width * 0.9, 700);
        var aspect = watermarkImg.naturalHeight / watermarkImg.naturalWidth;
        var wmHeight = wmWidth * aspect;
        var x = (canvas.width - wmWidth) / 2;
        var y = (canvas.height - wmHeight) / 2;

        ctx.save();
        ctx.globalAlpha = 0.08;
        ctx.drawImage(watermarkImg, x, y, wmWidth, wmHeight);
        ctx.restore();
    },

    captureAvatarToBase64: async function () {
        if (!this.displayStream && !this._displayCaptureAttempted) {
            this._displayCaptureAttempted = true;
            await this.initDisplayCapture();
        }

        var canvas;

        if (this.displayStream && this.displayVideo && this.displayVideo.readyState >= 2) {
            canvas = await this.captureWithDisplayMedia();
        } else {
            canvas = await this.captureWithHtml2canvas();
        }

        await this.applyWatermark(canvas);
        return canvas.toDataURL('image/png');
    },

    initDisplayCaptureEarly: async function () {
        if (this.displayStream || this._displayCaptureAttempted) return;
        this._displayCaptureAttempted = true;
        await this.initDisplayCapture();
    }
};
