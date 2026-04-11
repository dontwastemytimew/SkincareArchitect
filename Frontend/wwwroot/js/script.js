let productQueue = [];

function addProduct() {
    const nameInput = document.getElementById('prodName');
    const typeInput = document.getElementById('ingType');

    if (!nameInput.value) return;

    productQueue.push({
        name: nameInput.value,
        type: typeInput.value
    });

    nameInput.value = '';
    updateQueueUI();
}

function undoProduct() {
    productQueue.pop();
    updateQueueUI();
}

function updateQueueUI() {
    const container = document.getElementById('queueList');
    container.innerHTML = productQueue.map(p => `
        <div class="queue-item">
            <span>${p.name}</span>
            <small style="opacity: 0.7;">${p.type}</small>
        </div>
    `).join('');
}

async function runAnalysis() {
    if (productQueue.length < 2) {
        alert("додайте хоча б два засоби для аналізу");
        return;
    }

    const reportArea = document.getElementById('reportArea');
    reportArea.style.display = 'block';
    reportArea.innerText = "аналізуємо склад... (задіяно 12 патернів)";

    try {
        const response = await fetch('http://localhost:5016/api/skincare/test-conflict');
        const data = await response.json();

        reportArea.innerText = data.analysis;
    } catch (e) {
        reportArea.innerText = "помилка: бекенд не відповідає";
    }
}