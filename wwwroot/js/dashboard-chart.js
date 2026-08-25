window.shabakatDashboardChart = (function () {
  var instances = {};

  function formatCurrency(value) {
    return new Intl.NumberFormat("en-US", {
      style: "currency",
      currency: "USD",
      minimumFractionDigits: 2,
      maximumFractionDigits: 2
    }).format(Number(value) || 0);
  }

  function formatCompact(value) {
    return new Intl.NumberFormat("en-US", {
      style: "currency",
      currency: "USD",
      notation: "compact",
      maximumFractionDigits: 1
    }).format(Number(value) || 0);
  }

  function cssVar(name, fallback) {
    try {
      var value = getComputedStyle(document.documentElement).getPropertyValue(name).trim();
      return value || fallback;
    } catch (_) {
      return fallback;
    }
  }

  function destroy(canvasId) {
    var existing = instances[canvasId];
    if (existing) {
      existing.destroy();
      delete instances[canvasId];
    }
  }

  function render(canvasId, labels, values, colors) {
    var canvas = document.getElementById(canvasId);
    if (!canvas || typeof Chart === "undefined") {
      return false;
    }

    destroy(canvasId);

    var card = cssVar("--card", "#16161f");
    var border = cssVar("--border", "rgba(255,255,255,0.08)");
    var foreground = cssVar("--foreground", "#f4f4f5");
    var muted = "#7a7a9a";

    instances[canvasId] = new Chart(canvas, {
      type: "bar",
      data: {
        labels: labels,
        datasets: [{
          data: values,
          backgroundColor: colors,
          hoverBackgroundColor: colors,
          borderRadius: { topLeft: 10, topRight: 10, bottomLeft: 0, bottomRight: 0 },
          borderSkipped: false,
          maxBarThickness: 72,
          categoryPercentage: 0.72,
          barPercentage: 0.9
        }]
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        layout: {
          padding: { top: 6, right: 6, left: 0, bottom: 0 }
        },
        animation: { duration: 450 },
        plugins: {
          legend: { display: false },
          tooltip: {
            backgroundColor: card,
            borderColor: border,
            borderWidth: 1,
            titleColor: muted,
            bodyColor: foreground,
            cornerRadius: 16,
            padding: 12,
            displayColors: false,
            boxPadding: 4,
            caretPadding: 8,
            callbacks: {
              title: function (items) {
                return items[0] ? items[0].label : "";
              },
              label: function (context) {
                return formatCurrency(context.parsed.y);
              }
            }
          }
        },
        scales: {
          x: {
            grid: { display: false, drawBorder: false },
            border: { display: false },
            ticks: { color: muted, font: { size: 12 } }
          },
          y: {
            beginAtZero: true,
            grace: "8%",
            grid: {
              color: "rgba(255,255,255,0.05)",
              drawBorder: false,
              borderDash: [3, 3]
            },
            border: { display: false },
            ticks: {
              color: muted,
              font: { size: 11 },
              callback: function (value) {
                return formatCompact(value);
              }
            }
          }
        }
      }
    });

    return true;
  }

  return { render: render, destroy: destroy };
})();
