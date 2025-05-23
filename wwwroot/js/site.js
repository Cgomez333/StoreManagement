        document.addEventListener('DOMContentLoaded', async () => {
          const formatPercent = (current, previous) => {
            if (previous === 0) return '0.0%';
            const pct = ((current - previous) / previous) * 100;
            return pct.toFixed(1) + '%';
          };

          try {
            const [
              salesByDay,
              prevRev,
              prevOrders,
              avgProd,
              salesByProd,
              topCust,
              prevAvgOrder
            ] = await Promise.all([
              fetch('/Dashboard/MtdSalesByDay').then(r => r.json()),
              fetch('/Dashboard/PreviousMonthRevenue').then(r => r.json()),
              fetch('/Dashboard/PreviousMonthOrdersCount').then(r => r.json()),
              fetch('/Dashboard/AverageProductValue').then(r => r.json()),
              fetch('/Dashboard/SalesByProduct').then(r => r.json()),
              fetch('/Dashboard/TopCustomers').then(r => r.json()),
              fetch('/Dashboard/PreviousAvgOrderValue').then(r => r.json())
            ]);

            // Ingresos MTD
            const ingresos = salesByDay.reduce((s, d) => s + d.amount, 0);
            document.getElementById('mtdRevenue').textContent =
              ingresos.toLocaleString('es-CO', { style: 'currency', currency: 'COP' });
            document.getElementById('mtdRevenueCompare').innerHTML =
              `<span class="tag">${formatPercent(ingresos, prevRev.totalPrev)} vs mes pasado ~ <strong>${prevRev.totalPrev.toLocaleString('es-CO', { style: 'currency', currency: 'COP' })}</strong></span>`;

            new Chart(document.getElementById('chartMtdSalesByDay').getContext('2d'), {
              type: 'line',
              data: {
                labels: salesByDay.map(d => d.date),
                datasets: [{
                  data: salesByDay.map(d => d.amount),
                  borderColor: '#1ab188',
                  backgroundColor: 'rgba(26,177,136,0.2)',
                  fill: true,
                  tension: 0.4,
                  pointRadius: 6,
                  pointBackgroundColor: '#fff',
                  pointBorderColor: '#1ab188',
                  pointBorderWidth: 2,
                }]
              },
              options: {
                scales: {
                  x: { grid: { display: false } },
                  y: {
                    grid: { color: '#f0f0f0' },
                    ticks: { callback: v => v.toLocaleString('es-CO') }
                  }
                },
                plugins: { legend: { display: false } }
              }
            });

            // Órdenes procesadas
            const count = salesByDay.length;
            document.getElementById('ordersCount').textContent = count;
            document.getElementById('ordersCountCompare').innerHTML =
              `<span class="tag">${formatPercent(count, prevOrders.countPrev)} vs mes pasado ~ <strong>${prevOrders.countPrev}</strong></span>`;

            // Valor promedio productos
            document.getElementById('avgProductValue').textContent =
              avgProd.avg.toLocaleString('es-CO', { style: 'currency', currency: 'COP' });

            // Valor promedio órdenes
            const avgOrder = count ? ingresos / count : 0;
            document.getElementById('avgOrderValue').textContent =
              avgOrder.toLocaleString('es-CO', { style: 'currency', currency: 'COP' });
            document.getElementById('avgOrderValueCompare').innerHTML =
              `<span class="tag">${formatPercent(avgOrder, prevAvgOrder.avgPrev)} vs mes pasado ~ <strong>${prevAvgOrder.avgPrev.toLocaleString('es-CO', { style: 'currency', currency: 'COP' })}</strong></span>`;

            // Top 10 Productos
            new Chart(document.getElementById('chartSalesByProduct').getContext('2d'), {
              type: 'bar',
              data: {
                labels: salesByProd.map(d => d.product),
                datasets: [{
                  data: salesByProd.map(d => d.subtotal),
                  backgroundColor: '#e07a5f',
                  borderRadius: 8,
                  barPercentage: 0.6
                }]
              },
              options: {
                indexAxis: 'y',
                scales: {
                  x: { grid: { color: '#f0f0f0' }, ticks: { callback: v => v.toLocaleString('es-CO') } },
                  y: { grid: { display: false } }
                },
                plugins: { legend: { display: false } }
              }
            });

            // Mejores Clientes
            document.getElementById('tblTopCustomers').innerHTML =
              topCust.map(c => `
                <tr>
                  <td>${c.customer}</td>
                  <td class="text-end">${c.total.toLocaleString('es-CO', { style: 'currency', currency: 'COP' })}</td>
                </tr>
              `).join('');

            // Tendencia de Órdenes
            const counts = salesByDay.map((_, i) => i + 1);
            new Chart(document.getElementById('chartOrdersTrend').getContext('2d'), {
              type: 'line',
              data: {
                labels: salesByDay.map(d => d.date),
                datasets: [{
                  data: counts,
                  borderColor: '#dda20a',
                  backgroundColor: 'rgba(221,162,10,0.2)',
                  fill: true,
                  tension: 0.3,
                  pointRadius: 5,
                  pointBackgroundColor: '#f6c23e',
                  pointHoverRadius: 7
                }]
              },
              options: {
                scales: {
                  x: { grid: { display: false } },
                  y: {
                    grid: { color: '#f0f0f0' },
                    ticks: { stepSize: 1 }
                  }
                },
                plugins: {
                  legend: { display: false },
                  tooltip: {
                    callbacks: {
                      label: ctx => `Órdenes: ${ctx.parsed.y}`
                    }
                  }
                }
              }
            });

          } catch (err) {
            console.error('Error cargando dashboard:', err);
          }
        });