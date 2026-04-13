const mockProducts = [
    { id: 1, name: "The Ordinary Retinol 0.5%", type: "Retinoid", texture: "oil", time: "evening" },
    { id: 2, name: "CeraVe Hydrating Cleanser", type: "Moisturizer", texture: "gel", time: "both" },
    { id: 3, name: "The Ordinary Glycolic Acid 7%", type: "Acid", texture: "liquid", time: "evening" },
    { id: 4, name: "La Roche-Posay SPF 50", type: "SPF", texture: "cream", time: "morning" },
    { id: 5, name: "Skin1004 Centella Ampoule", type: "Soothing", texture: "liquid", time: "both" }
];

let currentLang = localStorage.getItem('selectedLang') || 'uk';
let isLogged = false;
let userName = "";
let analysisQueue = [];
let myShelf = [];
let selectedSource = 'all';

// Локалізація
async function changeLang(lang) {
    localStorage.setItem('selectedLang', lang);
    currentLang = lang;
    await applyTranslations();
    const panel = document.getElementById('settingsPanel');
    if (panel) panel.style.display = 'none';
}

async function applyTranslations() {
    try {
        const response = await fetch(`http://localhost:5016/api/skincare/translations?v=${Date.now()}`, {
            headers: {
                'Accept-Language': currentLang,
                'Cache-Control': 'no-cache'
            }
        });
        const translations = await response.json();

        document.querySelectorAll('[data-i18n], [data-i18n-placeholder]').forEach(el => {
            const key = el.getAttribute('data-i18n');
            const phKey = el.getAttribute('data-i18n-placeholder');

            if (key && translations[key]) {
                if (el.tagName === 'SELECT') {
                    if (el.options.length > 0) el.options[0].text = translations[key];
                } else {
                    const icon = el.querySelector('i');
                    if (icon) {
                        el.innerHTML = '';
                        el.appendChild(icon);
                        el.insertAdjacentText('beforeend', ' ' + translations[key]);
                    } else {
                        el.innerText = translations[key];
                    }
                }
            }
            if (phKey && translations[phKey]) {
                el.setAttribute('placeholder', translations[phKey]);
            }
        });
    } catch (e) {
        console.error("Помилка перекладу:", e);
    }
}

// Навігація
function showConstructor() {
    hideAllSections();
    document.getElementById('constructorSection').style.display = 'flex';
    updateNavActive(0);
}

function addToShelf(id, buttonElement) {
    if (!isLogged) {
        showAlert(currentLang === 'uk' ? "Спочатку увійдіть!" : "Please login first!");
        login();
        return;
    }

    const product = mockProducts.find(p => p.id === id);

    if (!myShelf.some(p => p.id === id)) {
        myShelf.push(product);
        
        if (buttonElement) {
            buttonElement.classList.add('btn-catalog-added');
            buttonElement.innerHTML = `<i class="fas fa-check"></i> ${currentLang === 'uk' ? 'Додано' : 'Added'}`;
        }
        
        renderFullShelf();
    }
}

function showMyShelf() {
    if (!isLogged) {
        login();
        return;
    }
    hideAllSections();
    document.getElementById('shelfSection').style.display = 'block';
    updateNavActive(1);
    renderFullShelf();
}

function showAbout() {
    hideAllSections();
    document.getElementById('aboutSection').style.display = 'block';
    updateNavActive(3);
}

function hideAllSections() {
    document.getElementById('constructorSection').style.display = 'none';
    document.getElementById('shelfSection').style.display = 'none';
    document.getElementById('aboutSection').style.display = 'none';
}

function updateNavActive(index) {
    const navLinks = document.querySelectorAll('.top-nav a');
    navLinks.forEach(link => link.classList.remove('active'));
    if(navLinks[index]) navLinks[index].classList.add('active');
}

// Логіка продуктів
function setSource(source) {
    selectedSource = source;
    const btnAll = document.getElementById('btnAll');
    const btnShelf = document.getElementById('btnShelf');
    if(btnAll) btnAll.classList.toggle('active', source === 'all');
    if(btnShelf) btnShelf.classList.toggle('active', source === 'shelf');

    const select = document.getElementById('prodSelect');
    if (!select) return;

    select.innerHTML = '<option value="" data-i18n="SelectPlaceholder">— виберіть баночку —</option>';

    if (source === 'shelf') {
        if (!isLogged || myShelf.length === 0) {
            showAlert(currentLang === 'uk' ? "Ваша поличка порожня!" : "Your shelf is empty!");
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
    applyTranslations();
}

function addProductToAnalysis() {
    const select = document.getElementById('prodSelect');
    const id = select.value;
    if (!id) return;

    const sourceArray = selectedSource === 'all' ? mockProducts : myShelf;
    const product = sourceArray.find(p => p.id == id);

    if (product && !analysisQueue.some(p => p.id === product.id)) {
        analysisQueue.push(product);
        updateAnalysisUI();
    }
}

function updateAnalysisUI() {
    const container = document.getElementById('queueList');
    container.innerHTML = analysisQueue.map((p, index) => `
        <div class="product-card-mini" onclick="removeFromAnalysis(${index})" title="Клікніть, щоб прибрати">
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

// Аналіз
async function runAnalysis() {
    if (analysisQueue.length < 2) {
        const msg = currentLang === 'uk' ? "додайте хоча б два засоби" : "add at least two products";
        showAlert(msg);
        return;
    }

    const reportArea = document.getElementById('reportArea');
    reportArea.style.display = 'block';
    reportArea.innerHTML = "<em>loading...</em>";

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
            .replace(/--- ANALYSIS RESULT ---/g, '')
            .replace(/\[SkincareArchitect 2026\]/g, '')
            .trim();
        
        reportArea.innerHTML = `
            <div class="analysis-result-header" data-i18n="RoutineHeader">РЕЗУЛЬТАТ АНАЛІЗУ</div>
            <div class="analysis-main-msg">${cleanAnalysis}</div>
            <div class="routine-recommendation">
                <div class="routine-block">
                    <p><i class="fas fa-sun" style="color: #f1c40f;"></i> <strong data-i18n="MorningTitle">РАНОК</strong></p>
                    <p class="routine-path">${routine.morning.length > 0 ? routine.morning.map(p => p.name).join(' → ') : '---'}</p>
                </div>
                <div class="routine-block" style="margin-top: 15px;">
                    <p><i class="fas fa-moon" style="color: #f39c12;"></i> <strong data-i18n="EveningTitle">ВЕЧІР</strong></p>
                    <p class="routine-path">${routine.evening.length > 0 ? routine.evening.map(p => p.name).join(' → ') : '---'}</p>
                </div>
            </div>
        `;
        
        await applyTranslations();

    } catch (e) {
        reportArea.innerText = "Error: Server not responding";
    }
}

function getRoutineSuggestion() {
    const morning = analysisQueue.filter(p => p.time === 'morning' || p.time === 'both');
    const evening = analysisQueue.filter(p => p.time === 'evening' || p.time === 'both');
    const order = {"liquid": 1, "gel": 2, "oil": 3, "cream": 4};
    const sortByTexture = (a, b) => (order[a.texture] || 99) - (order[b.texture] || 99);
    return { morning: morning.sort(sortByTexture), evening: evening.sort(sortByTexture) };
}

// Auth & Init
function login() {
    userName = prompt(currentLang === 'uk' ? "Як вас звати?" : "What is your name?", "Марина");
    if (userName) {
        isLogged = true;
        const btn = document.querySelector('.auth-btn');
        btn.innerHTML = `<i class="fas fa-user"></i> ${userName}`;
        btn.onclick = null;
    }
}

window.onload = async () => {
    await applyTranslations();
    setSource('all');
};

function showAlert(message, title = "Skincare Architect") {
    document.getElementById('alertTitle').innerText = title;
    document.getElementById('alertMessage').innerText = message;
    document.getElementById('customAlert').style.display = 'flex';
}

function closeAlert() {
    document.getElementById('customAlert').style.display = 'none';
}

function login() {
    document.getElementById('loginModal').style.display = 'flex';
    document.getElementById('loginName').focus();
}

function closeLogin() {
    document.getElementById('loginModal').style.display = 'none';
}

function confirmLogin() {
    const nameInput = document.getElementById('loginName');
    if (nameInput.value.trim()) {
        userName = nameInput.value;
        isLogged = true;
        
        const btn = document.querySelector('.auth-btn');
        btn.innerHTML = `<i class="fas fa-user"></i> ${userName}`;
        btn.onclick = null;

        document.getElementById('logoutBtn').style.display = 'inline-block';
        closeLogin();

        const msg = currentLang === 'uk' ? `Вітаємо, ${userName}!` : `Welcome, ${userName}!`;
        showAlert(msg);
    }
}

function logout() {
    isLogged = false;
    userName = "";
    analysisQueue = [];
    myShelf = [];

    // Оновлюємо кнопку профілю
    const btn = document.querySelector('.auth-btn');
    btn.innerHTML = '';
    btn.onclick = login;

    document.getElementById('logoutBtn').style.display = 'none';
    document.getElementById('loginName').value = '';
    
    if (document.getElementById('catalogSection').style.display === 'block') {
        renderCatalog();
    }
    
    if (document.getElementById('shelfSection').style.display === 'block') {
        renderFullShelf();
    }

    showConstructor();
    applyTranslations();
}

function showCatalog() {
    hideAllSections();
    document.getElementById('catalogSection').style.display = 'block';
    updateNavActive(2); // Індекс 2 для каталогу
    renderCatalog();
}

function hideAllSections() {
    document.getElementById('constructorSection').style.display = 'none';
    document.getElementById('shelfSection').style.display = 'none';
    document.getElementById('aboutSection').style.display = 'none';
    document.getElementById('catalogSection').style.display = 'none';
}

function renderCatalog() {
    const container = document.getElementById('catalogGrid');
    if (!container) return;
    const btnText = currentLang === 'uk' ? "на поличку" : "to shelf";
    const addedText = currentLang === 'uk' ? "Додано" : "Added";

    container.innerHTML = mockProducts.map(p => {
        const alreadyInShelf = myShelf.some(item => item.id === p.id);
        const btnClass = alreadyInShelf ? "btn-catalog-add btn-catalog-added" : "btn-catalog-add";
        const label = alreadyInShelf ? addedText : btnText;
        const icon = alreadyInShelf ? "fa-check" : "fa-plus";

        return `
        <div class="catalog-card">
            <div class="catalog-img"><i class="fas fa-pump-soap"></i></div>
            <h4>${p.name}</h4>
            <small>${p.type}</small>
            <button class="${btnClass}" onclick="addToShelf(${p.id}, this)">
                <i class="fas ${icon}"></i> ${label}
            </button>
        </div>`;
    }).join('');
}

function renderFullShelf() {
    const container = document.getElementById('fullShelfDisplay');
    const userTitle = document.getElementById('shelfUserName');

    if (!container) return;

    userTitle.innerText = currentLang === 'uk' ? `Колекція: ${userName}` : `Collection: ${userName}`;

    if (myShelf.length === 0) {
        container.innerHTML = `
            <div style="grid-column: 1/-1; text-align: center; padding: 40px;">
                <i class="fas fa-ghost" style="font-size: 3rem; color: #ccc; margin-bottom: 15px;"></i>
                <p>${currentLang === 'uk' ? 'Тут поки порожньо. Додайте щось із каталогу!' : 'Empty here. Add something from catalog!'}</p>
            </div>`;
        return;
    }
    
    container.innerHTML = myShelf.map((p, index) => `
        <div class="catalog-card">
            <button class="btn-remove-shelf" onclick="removeFromShelf(${index})" title="${currentLang === 'uk' ? 'Прибрати з полички' : 'Remove from shelf'}">
                <i class="fas fa-times"></i>
            </button>
            
            <div class="catalog-img">
                <i class="fas fa-wine-bottle"></i>
            </div>
            <h4>${p.name}</h4>
            <small>${p.type}</small>
            
            <div style="font-size: 0.7rem; color: var(--silver-blue); margin-top: 10px;">
                <i class="fas fa-clock"></i> ${p.time === 'both' ? (currentLang === 'uk' ? 'Ранок та вечір' : 'Morning & Evening') : (p.time === 'morning' ? (currentLang === 'uk' ? 'Ранок' : 'Morning') : (currentLang === 'uk' ? 'Вечір' : 'Evening'))}
            </div>
        </div>
    `).join('');
}

function removeFromShelf(index) {
    myShelf.splice(index, 1);
    renderFullShelf();
    if (document.getElementById('catalogSection').style.display === 'block') {
        renderCatalog();
    }
}