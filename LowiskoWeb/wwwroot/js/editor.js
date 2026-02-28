window.mapaEditor = {
    getContainerRect: function (el) {
        const r = el.getBoundingClientRect();
        return { x: r.left, y: r.top, w: r.width, h: r.height };
    }
};

(function loadPogodaHome() {
    const codes = {0:'☀️',1:'🌤️',2:'⛅',3:'☁️',45:'🌫️',48:'🌫️',51:'🌦️',53:'🌧️',55:'🌧️',61:'🌧️',63:'🌧️',65:'🌧️',71:'🌨️',73:'🌨️',75:'🌨️',80:'🌦️',81:'🌧️',82:'🌧️',95:'⛈️',96:'⛈️',99:'⛈️'};
    const dni = ['Nd','Pn','Wt','Śr','Cz','Pt','Sb'];
    function render() {
        const el = document.getElementById('pogoda-home');
        if (!el) { setTimeout(render, 500); return; }
        fetch('https://api.open-meteo.com/v1/forecast?latitude=54.6122&longitude=18.0823&current=temperature_2m,weathercode,windspeed_10m&daily=temperature_2m_max,temperature_2m_min,weathercode&timezone=Europe/Warsaw&forecast_days=4')
            .then(r => r.json())
            .then(d => {
                const cur = d.current;
                const ico = codes[cur.weathercode] || '🌡️';
                let h = '<div class="pogoda-home-now">';
                h += '<div class="pogoda-home-ico">' + ico + '</div>';
                h += '<div class="pogoda-home-info">';
                h += '<div class="pogoda-home-temp">' + Math.round(cur.temperature_2m) + '°C</div>';
                h += '<div class="pogoda-home-wiatr">💨 ' + Math.round(cur.windspeed_10m) + ' km/h</div>';
                h += '</div></div>';
                h += '<div class="pogoda-home-dni">';
                for (let i = 1; i < 4; i++) {
                    const dt = new Date(d.daily.time[i]);
                    const dn = dni[dt.getDay()];
                    const ic2 = codes[d.daily.weathercode[i]] || '🌡️';
                    h += '<div class="pogoda-home-d"><span class="pogoda-home-dn">' + dn + '</span><span>' + ic2 + '</span><span class="pogoda-home-mm">' + Math.round(d.daily.temperature_2m_max[i]) + '°/' + Math.round(d.daily.temperature_2m_min[i]) + '°</span></div>';
                }
                h += '</div>';
                el.innerHTML = h;
            })
            .catch(() => { el.innerHTML = '<div class="pogoda-home-loading">Brak pogody</div>'; });
    }
    if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', render);
    else setTimeout(render, 300);
})();
