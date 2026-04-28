let mockProducts = [];

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

// Завантажує переклади з бекенду та замінює текст у всіх елементах
async function applyTranslations() {
    try {
        const response = await fetch(`http://localhost:5016/api/skincare/translations?v=${Date.now()}`, {
            headers: { 'Accept-Language': currentLang }
        });
        const translations = await response.json();

        document.querySelectorAll('[data-i18n], [data-i18n-placeholder]').forEach(el => {
            const key = el.getAttribute('data-i18n');
            const phKey = el.getAttribute('data-i18n-placeholder');

            if (key && translations[key]) {
                if (key === 'AuthLogin' && isLogged) {
                    el.innerHTML = `<i class="fas fa-user"></i> ${userName}`;
                    return;
                }

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

// Оновлює візуальне підсвічування активного пункту в меню
function updateNavActive(index) {
    const navLinks = document.querySelectorAll('.top-nav a');
    navLinks.forEach(link => link.classList.remove('active'));
    if(navLinks[index]) navLinks[index].classList.add('active');
}

// Перемикає джерело пошуку в конструкторі
function setSource(source) {
    selectedSource = source;
    const btnAll = document.getElementById('btnAll');
    const btnShelf = document.getElementById('btnShelf');
    if(btnAll) btnAll.classList.toggle('active', source === 'all');
    if(btnShelf) btnShelf.classList.toggle('active', source === 'shelf');

    const searchInput = document.getElementById('constructorSearch');
    if (searchInput) {
        searchInput.value = ''; // Очищаємо поле при перемиканні
    }

    if (source === 'shelf') {
        if (!isLogged || myShelf.length === 0) {
            showAlert(currentLang === 'uk' ? "Ваша поличка порожня!" : "Your shelf is empty!");
            setSource('all');
            return;
        }
        renderConstructorSearch();
    } else {
        document.getElementById('constructorSearchResults').innerHTML = '';
    }
}

// Додає обраний продукт у чергу на аналіз
function addProductToAnalysis(id) {
    const sourceArray = selectedSource === 'all' ? mockProducts : myShelf;
    const product = sourceArray.find(p => p.id == id || p.id === parseInt(id)); // parseInt на випадок, якщо id число

    if (product) {
        if (!analysisQueue.some(p => p.id === product.id)) {
            analysisQueue.push(product);
            updateAnalysisUI();
            
            document.getElementById('constructorSearch').value = '';
            document.getElementById('constructorSearchResults').innerHTML = '';
        } else {
            showAlert(currentLang === 'uk' ? "Цей засіб вже у черзі!" : "Already in queue!");
        }
    }
}

// Оновлює візуальний список обраних продуктів у конструкторі
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

// Видаляє продукт із черги на аналіз по кліку
function removeFromAnalysis(index) {
    analysisQueue.splice(index, 1);
    updateAnalysisUI();
}

// Відправляє зібрану рутину на бекенд для перевірки сумісності
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

// Сортує засоби з черги за текстурою та часом використання (Ранок/Вечір)
function getRoutineSuggestion() {
    const morning = analysisQueue.filter(p => p.time === 'morning' || p.time === 'both');
    const evening = analysisQueue.filter(p => p.time === 'evening' || p.time === 'both');
    
    const sortByStep = (a, b) => (a.order || 3) - (b.order || 3);

    return {
        morning: morning.sort(sortByStep),
        evening: evening.sort(sortByStep)
    };
}

window.onload = async () => {
    await applyTranslations();
    
    try {
        const response = await fetch('http://localhost:5016/api/skincare/products');
        const data = await response.json();
        
        mockProducts = data;
        
        setSource('all');

        console.log(`Завантажено ${mockProducts.length} реальних продуктів з бази Sephora`);
    } catch (e) {
        console.error("Помилка завантаження бази продуктів:", e);
    }
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

// Зберігає ім'я користувача та оновлює шапку
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
    updateNavActive(2);
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
    
    const searchTerm = (document.getElementById('searchInput')?.value || '').toLowerCase().trim();
    const btnText = currentLang === 'uk' ? "на поличку" : "to shelf";
    const addedText = currentLang === 'uk' ? "Додано" : "Added";
    
    if (searchTerm.length === 0) {
        container.style.display = 'block';
        container.innerHTML = `
            <div style="text-align: center; padding: 40px; color: var(--lapis-lazuli);">
                <i class="fas fa-search" style="font-size: 3rem; color: #ccc; margin-bottom: 15px;"></i>
                <p>${currentLang === 'uk' ? 'Введіть назву бренду або засобу для пошуку...' : 'Type a brand or product name to search...'}</p>
            </div>`;
        return;
    }
    
    container.style.display = 'grid';
    
    let filteredProducts = mockProducts.filter(p =>
        p.name.toLowerCase().includes(searchTerm) ||
        (p.brand && p.brand.toLowerCase().includes(searchTerm)) ||
        p.type.toLowerCase().includes(searchTerm)
    );
    
    if (filteredProducts.length === 0) {
        container.style.display = 'block';
        container.innerHTML = `
            <div style="text-align: center; padding: 40px; color: var(--lapis-lazuli);">
                <i class="fas fa-box-open" style="font-size: 3rem; color: #ccc; margin-bottom: 15px;"></i>
                <p>${currentLang === 'uk' ? 'Нічого не знайдено за запитом' : 'Nothing found for'} "<strong>${searchTerm}</strong>"</p>
            </div>`;
        return;
    }
    
    const displayProducts = filteredProducts.slice(0, 50);

    container.innerHTML = displayProducts.map(p => {
        const alreadyInShelf = myShelf.some(item => item.id === p.id);
        const btnClass = alreadyInShelf ? "btn-catalog-add btn-catalog-added" : "btn-catalog-add";
        const label = alreadyInShelf ? addedText : btnText;
        const icon = alreadyInShelf ? "fa-check" : "fa-plus";

        return `
        <div class="catalog-card">
            <div class="catalog-img"><i class="fas fa-pump-soap"></i></div>
            <h4>${p.name}</h4>
            <small>${p.brand || 'Skincare'} • ${p.type}</small>
            <button class="${btnClass}" onclick="addToShelf('${p.id}', this)">
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

// Запускає паралельний та послідовний аналіз бази на бекенді (вимірювання швидкості)
async function runBenchmark() {
    const btn = document.getElementById('btnBenchmark');
    const resultsDiv = document.getElementById('benchmarkResults');

    btn.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Аналізуємо...';
    btn.disabled = true;
    resultsDiv.style.display = 'none';

    try {
        const response = await fetch('http://localhost:5016/api/benchmark/run');
        const data = await response.json();
        
        resultsDiv.innerHTML = `
            <strong>Оброблено продуктів:</strong> ${data.totalProducts} шт.<br>
            <strong>Послідовно (1 потік):</strong> ${data.sequentialTimeMs} мс<br>
            <strong>Паралельно (${data.coresUsed} ядер):</strong> ${data.parallelTimeMs} мс<br>
            <strong style="color: #27ae60; font-size: 1.1rem;">Прискорення: ${data.speedUp}</strong>
        `;
        resultsDiv.style.display = 'block';
    } catch (error) {
        console.error("Помилка бенчмарку:", error);
        resultsDiv.innerHTML = '<span style="color: red;">Помилка підключення до сервера</span>';
        resultsDiv.style.display = 'block';
    } finally {
        btn.innerHTML = 'Запустити аналіз бази';
        btn.disabled = false;
    }
}

function renderConstructorSearch() {
    const container = document.getElementById('constructorSearchResults');
    const term = document.getElementById('constructorSearch').value.toLowerCase().trim();
    const sourceArray = selectedSource === 'all' ? mockProducts : myShelf;
    
    if (term.length === 0) {
        if (selectedSource === 'all') {
            container.innerHTML = '';
            return;
        } else {
            renderListToConstructor(myShelf, container);
            return;
        }
    }
    
    const filtered = sourceArray.filter(p =>
        p.name.toLowerCase().includes(term) ||
        (p.brand && p.brand.toLowerCase().includes(term))
    );

    if (filtered.length === 0) {
        container.innerHTML = `<div style="text-align:center; padding: 10px; font-size: 0.9rem; color: var(--lapis-lazuli);">${currentLang === 'uk' ? 'Не знайдено' : 'Not found'}</div>`;
        return;
    }
    
    const displayLimit = selectedSource === 'all' ? filtered.slice(0, 10) : filtered;
    renderListToConstructor(displayLimit, container);
}

// Керування панеллю адміністратора
function openAdminPanel() {
    document.getElementById('adminModal').style.display = 'flex';
}

function closeAdminPanel() {
    document.getElementById('adminModal').style.display = 'none';
    document.getElementById('benchmarkResults').style.display = 'none';
    document.getElementById('benchmarkResults').innerHTML = '';
}

function renderListToConstructor(list, container) {
    container.innerHTML = list.map(p => `
        <div class="constructor-result-item" onclick="addProductToAnalysis('${p.id}')">
            <div>
                <strong style="color:var(--burgundy); font-size:0.9rem;">${p.name}</strong><br>
                <small style="color:var(--lapis-lazuli);">${p.brand || 'Skincare'} • ${p.type}</small>
            </div>
            <i class="fas fa-plus" style="color:var(--burgundy);"></i>
        </div>
    `).join('');
}
