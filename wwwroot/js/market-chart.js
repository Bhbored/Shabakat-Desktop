window.shabakatMarketChart = (function () {
  var instances = {};

  function destroy(canvasId) {
    if (instances[canvasId]) {
      instances[canvasId].destroy();
      delete instances[canvasId];
    }
  }

  function cssVar(name, fallback) {
    try { return getComputedStyle(document.documentElement).getPropertyValue(name).trim() || fallback; }
    catch (_) { return fallback; }
  }

  function render(canvasId, labels, series, unit, locale) {
    var canvas = document.getElementById(canvasId);
    if (!canvas || typeof Chart === "undefined") return false;
    destroy(canvasId);
    var reducedMotion = window.matchMedia && window.matchMedia("(prefers-reduced-motion: reduce)").matches;
    var foreground = cssVar("--foreground", "#f4f4f5");
    var border = cssVar("--border", "rgba(255,255,255,.08)");
    var card = cssVar("--card", "#16161f");
    var datasets = series.map(function (item) {
      return {
        label: item.label,
        data: item.values,
        borderColor: item.color,
        backgroundColor: item.color,
        borderWidth: 2,
        borderDash: item.dashLength ? [item.dashLength, item.dashLength] : [],
        pointRadius: 0,
        pointHoverRadius: 4,
        tension: 0.25
      };
    });
    instances[canvasId] = new Chart(canvas, {
      type: "line",
      data: { labels: labels, datasets: datasets },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        animation: reducedMotion ? false : { duration: 250 },
        interaction: { mode: "index", intersect: false },
        plugins: {
          legend: { display: datasets.length > 1, labels: { color: foreground, usePointStyle: true } },
          tooltip: {
            backgroundColor: card, borderColor: border, borderWidth: 1, titleColor: foreground, bodyColor: foreground,
            callbacks: { label: function (context) { return context.dataset.label + ": " + Number(context.parsed.y).toLocaleString(locale || "en") + " " + unit; } }
          }
        },
        scales: {
          x: { grid: { display: false }, border: { display: false }, ticks: { color: foreground, maxTicksLimit: 8 } },
          y: { grid: { color: border }, border: { display: false }, ticks: { color: foreground, callback: function (value) { return Number(value).toLocaleString(locale || "en", { notation: "compact" }); } } }
        }
      }
    });
    return true;
  }

  return { render: render, destroy: destroy };
})();
