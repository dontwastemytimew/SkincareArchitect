const mockProducts = [
    { id: 1, name: "The Ordinary Retinol 0.5%", type: "Retinoid", texture: "oil", time: "evening", img: "https://images.unsplash.com/photo-1620916566398-39f1143ab7be?auto=format&fit=crop&w=200&q=80" },
    { id: 2, name: "CeraVe Hydrating Cleanser", type: "Moisturizer", texture: "gel", time: "both", img: "https://images.unsplash.com/photo-1556228720-195a672e8a03?auto=format&fit=crop&w=200&q=80" },
    { id: 3, name: "The Ordinary Glycolic Acid 7%", type: "Acid", texture: "liquid", time: "evening", img: "https://images.unsplash.com/photo-1601049541289-9b1b7bbbfe19?auto=format&fit=crop&w=200&q=80" },
    { id: 4, name: "La Roche-Posay SPF 50", type: "SPF", texture: "cream", time: "morning", img: "https://images.unsplash.com/photo-1598440947619-2c35fc9aa908?auto=format&fit=crop&w=200&q=80" },
    { id: 5, name: "Skin1004 Centella Ampoule", type: "Soothing", texture: "liquid", time: "both", img: "https://images.unsplash.com/photo-1617897903246-719242758052?auto=format&fit=crop&w=200&q=80" }
];

let analysisQueue = [];
let myShelf = []; 
let selectedSource = 'all';
let currentLang = 'uk';

function changeLang(lang) {
    currentLang = lang;
    alert(lang === 'uk' ? "Мову змінено на українську" : "Language changed to English");
    toggleSettings();
}

window.onload = () => {
    setSource('all');
};

function toggleSettings() {
    const panel = document.getElementById('settingsPanel');
    panel.style.display = panel.style.display === 'none' ? 'block' : 'none';
}

function getRoutineSuggestion() {
    // Міняємо productQueue на analysisQueue
    const morning = analysisQueue.filter(p => p.time === 'morning' || p.time === 'both');
    const evening = analysisQueue.filter(p => p.time === 'evening' || p.time === 'both');

    const order = { "liquid": 1, "gel": 2, "oil": 3, "cream": 4 };
    const sortByTexture = (a, b) => (order[a.texture] || 99) - (order[b.texture] || 99);

    return {
        morning: morning.sort(sortByTexture),
        evening: evening.sort(sortByTexture)
    };
}

function login() {
    const btn = document.querySelector('.auth-btn');
    userName = prompt("Як вас звати?", "Марина");
    if (userName) {
        isLogged = true;
        btn.innerHTML = `<i class="fas fa-user"></i> ${userName}`;
        btn.onclick = null;
        alert(`Вітаємо, ${userName}! Тепер ви можете користуватися поличкою.`);
    }
}

let isLogged = false;
let userName = "";

function renderFullShelf() {
    const container = document.getElementById('fullShelfDisplay');
    document.getElementById('shelfUserName').innerText = `Колекція користувача: ${userName}`;

    if (myShelf.length === 0) {
        container.innerHTML = "<p style='text-align:center; padding:20px; color: var(--lapis-lazuli);'>На вашій поличці поки порожньо. Перейдіть до каталогу, щоб знайти улюблені засоби.</p>";
        return;
    }
    
    container.innerHTML = myShelf.map((p, index) => `
        <div class="product-card-mini" style="flex-direction: column; text-align: center; padding: 20px;">
            <div class="product-img-stub" style="margin: 0 0 10px 0; width: 80px; height: 80px; font-size: 2rem;">
                <i class="fas fa-wine-bottle"></i>
            </div>
            <span style="font-weight: 600; color: var(--burgundy);">${p.name}</span>
            <small style="color: var(--lapis-lazuli);">${p.type}</small>
            <button onclick="removeFromShelf(${index})" style="background:none; border:none; color:var(--burgundy); cursor:pointer; margin-top:10px; font-size:0.8rem;">видалити ✖</button>
        </div>
    `).join('');
}

function showAbout() {
    hideAllSections();
    document.getElementById('aboutSection').style.display = 'block';
    updateNavActive(3);
}

function showConstructor() {
    hideAllSections();
    document.getElementById('constructorSection').style.display = 'flex';
    updateNavActive(0);
}

function showMyShelf() {
    if (!isLogged) { alert("Спочатку увійдіть!"); login(); return; }
    hideAllSections();
    document.getElementById('shelfSection').style.display = 'block';
    updateNavActive(1);
    renderFullShelf();
}

function hideAllSections() {
    document.getElementById('constructorSection').style.display = 'none';
    document.getElementById('shelfSection').style.display = 'none';
    document.getElementById('aboutSection').style.display = 'none';
}

function updateNavActive(index) {
    const navLinks = document.querySelectorAll('.top-nav a');
    navLinks.forEach(link => link.classList.remove('active'));
    navLinks[index].classList.add('active');
}

function setSource(source) {
    selectedSource = source;
    
    document.getElementById('btnAll').classList.toggle('active', source === 'all');
    document.getElementById('btnShelf').classList.toggle('active', source === 'shelf');

    const select = document.getElementById('prodSelect');
    select.innerHTML = '<option value="">— виберіть баночку —</option>';

    if (source === 'shelf') {
        if (!isLogged || myShelf.length === 0) {
            alert("Ваша поличка порожня! Додайте продукти з каталогу (в розробці).");
            setSource('all');
            return;
        }
        myShelf.forEach(p => {
            let opt = document.createElement('option');
            opt.value = p.id;
            opt.innerHTML = p.name;
            select.appendChild(opt);
        });
    } else {
        mockProducts.forEach(p => {
            let opt = document.createElement('option');
            opt.value = p.id;
            opt.innerHTML = p.name;
            select.appendChild(opt);
        });
    }
}

function addProductToAnalysis() {
    const id = document.getElementById('prodSelect').value;
    if(!id) return;

    const sourceArray = selectedSource === 'all' ? mockProducts : myShelf;
    const product = sourceArray.find(p => p.id == id);

    if (!analysisQueue.some(p => p.id === product.id)) {
        analysisQueue.push(product);
        updateAnalysisUI();
    }
}

function updateAnalysisUI() {
    const container = document.getElementById('queueList');
    container.innerHTML = analysisQueue.map((p, index) => `
        <div class="product-card-mini" onclick="removeFromAnalysis(${index})">
            <div class="product-img-stub"><i class="fas fa-pump-soap"></i></div>
            <div style="display: flex; flex-direction: column;">
                <span style="font-weight: 600; color: var(--burgundy); font-size: 0.9rem;">${p.name}</span>
                <small style="color: var(--lapis-lazuli); font-size: 0.7rem;">${p.type}</small>
            </div>
        </div>
    `).join('');
}

function removeFromAnalysis(index) {
    analysisQueue.splice(index, 1);
    updateAnalysisUI();
}

function removeFromShelf(index) {
    myShelf.splice(index, 1);
    renderFullShelf();
}

async function runAnalysis() {
    if (analysisQueue.length < 2) {
        alert("додайте хоча б два засоби для аналізу");
        return;
    }

    const reportArea = document.getElementById('reportArea');
    reportArea.style.display = 'block';
    reportArea.innerHTML = "<em>аналізуємо склад на сервері...</em>";

    try {
        const response = await fetch('http://localhost:5016/api/skincare/analyze', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Accept-Language': currentLang
            },
            body: JSON.stringify(analysisQueue)
        });

        const data = await response.json();
        const routine = getRoutineSuggestion();
        
        const cleanAnalysis = data.analysis
            .replace(/--- РЕЗУЛЬТАТ АНАЛІЗУ ---/g, '')
            .replace(/\[SkincareArchitect 2026\]/g, '')
            .trim();

        let html = `
            <div class="analysis-result-header">РЕЗУЛЬТАТ АНАЛІЗУ</div>
            <div class="analysis-main-msg">${cleanAnalysis}</div>
            
            <div class="routine-recommendation">
                <h4 style="margin: 15px 0 10px 0; color: var(--cherry-pink); text-transform: uppercase; letter-spacing: 1px;">Рекомендація щодо нанесення:</h4>
                
                <div class="routine-block">
                    <p><i class="fas fa-sun" style="color: #f1c40f;"></i> <strong>РАНОК</strong></p>
                    <p class="routine-path">${routine.morning.length > 0 ? routine.morning.map(p => p.name).join(' <i class="fas fa-chevron-right" style="font-size: 0.6rem; margin: 0 5px;"></i> ') : 'тільки очищення'}</p>
                </div>

                <div class="routine-block" style="margin-top: 15px;">
                    <p><i class="fas fa-moon" style="color: #f39c12;"></i> <strong>ВЕЧІР</strong></p>
                    <p class="routine-path">${routine.evening.length > 0 ? routine.evening.map(p => p.name).join(' <i class="fas fa-chevron-right" style="font-size: 0.6rem; margin: 0 5px;"></i> ') : 'тільки зволоження'}</p>
                </div>
            </div>
        `;

        reportArea.innerHTML = html;

    } catch (e) {
        reportArea.innerText = "помилка: сервер не відповідає";
    }
}
