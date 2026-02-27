window.mapaEditor = {
    getContainerRect: function (el) {
        const r = el.getBoundingClientRect();
        return { x: r.left, y: r.top, w: r.width, h: r.height };
    }
};
