window.avatarAnim = {
    manualScale: null,

    setManualScale: function (value) {
        this.manualScale = value;
        if (this._lastAngles) {
            this.update(this._lastAngles);
        }
    },

    update: function (angles) {
        this._lastAngles = angles;
        var scale = this.manualScale;
        if (scale == null) {
            var container = document.querySelector('.avatar-container');
            scale = 2;
            if (container) {
                scale = Math.min(container.clientWidth / 180, container.clientHeight / 400) * 0.85;
                if (scale < 0.5) scale = 0.5;
                if (scale > 3) scale = 3;
            }
        }
        var bw = document.getElementById('avatar-wrapper');
        if (bw) {
            bw.style.transform =
                'perspective(800px) rotateY(' + angles.torsoY + 'deg) ' +
                'rotateZ(' + angles.torsoZ + 'deg) scale(' + scale + ')';
        }

        var al = document.getElementById('arm-left');
        if (al) al.style.transform = 'rotate(' + angles.leftShoulder + 'deg)';

        var ar = document.getElementById('arm-right');
        if (ar) ar.style.transform = 'rotate(' + angles.rightShoulder + 'deg)';

        var fl = document.getElementById('forearm-left');
        if (fl) fl.style.transform = 'rotate(' + angles.leftElbow + 'deg)';

        var fr = document.getElementById('forearm-right');
        if (fr) fr.style.transform = 'rotate(' + angles.rightElbow + 'deg)';

        var h = document.getElementById('head');
        if (h) h.style.transform = 'rotate(' + angles.headTilt + 'deg)';

        var uprightItems = document.querySelectorAll('.item.stay-upright.active');
        for (var i = 0; i < uprightItems.length; i++) {
            var item = uprightItems[i];
            var isLeft = item.closest('.arm-container.left');
            var baseRotation = isLeft ? 0 : 0;
            var shoulderAngle = isLeft ? angles.leftShoulder : angles.rightShoulder;
            var elbowAngle = isLeft ? angles.leftElbow : angles.rightElbow;
            item.style.transform = 'rotate(' + (baseRotation - shoulderAngle - elbowAngle) + 'deg)';
        }
    },

    resetToNeutral: function () {
        this.update({
            torsoY: 0, torsoZ: 0,
            leftShoulder: 0, leftElbow: 0,
            rightShoulder: 0, rightElbow: 0,
            headTilt: 0
        });
    }
};
