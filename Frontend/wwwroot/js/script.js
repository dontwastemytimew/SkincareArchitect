let mockProducts = [];
let translations = {};

let currentLang = localStorage.getItem('selectedLang') || 'uk';
let userName = localStorage.getItem('userName') || "";
let isLogged = !!userName;
let analysisQueue = [];
let myShelf = [];
let selectedSource = 'all';
let catalogPage = 1;
const itemsPerPage = 24;


// Повертає іконку залежно від типу продукту або його назви
function getProductIcon(product) {
    const name = product.name.toLowerCase();
    const type = product.type.toLowerCase();

    // Сонцезахист
    if (name.includes('spf') || name.includes('sunscreen') || name.includes('uv')) {
        return { icon: 'fa-sun', color: 'var(--burgundy)' };
    }
    // Креми
    if (name.includes('cream') || name.includes('moisturizer')) {
        return { icon: 'fa-mortar-pestle', color: 'var(--burgundy)' };
    }
    // Сироватки та Активи
    if (name.includes('serum') || name.includes('essence') || name.includes('liquid') || type === 'acid') {
        return { icon: 'fa-vial', color: 'var(--burgundy)' };
    }
    // Вмивання та Тоніки
    if (name.includes('toner') || name.includes('cleanser') || name.includes('wash') || name.includes('water')) {
        return { icon: 'fa-soap', color: 'var(--burgundy)' };
    }
    // Маски
    if (name.includes('mask')) {
        return { icon: 'fa-face-smile-beam', color: 'var(--burgundy)' };
    }

    // Стандартна іконка
    return { icon: 'fa-pump-soap', color: 'var(--burgundy)' };
}

// Локалізація
async function changeLang(lang) {
    localStorage.setItem('selectedLang', lang);
    currentLang = lang;
    await applyTranslations();

    const panel = document.getElementById('settingsPanel');
    if (panel) panel.style.display = 'none';
    
    const reportArea = document.getElementById('reportArea');
    
    if (reportArea && reportArea.style.display === 'block') {
        await runAnalysis();
    }
}

// Завантажує переклади з бекенду та замінює текст у всіх елементах
async function applyTranslations() {
    try {
        const response = await fetch(`http://localhost:5016/api/skincare/translations?v=${Date.now()}`, {
            headers: { 'Accept-Language': currentLang }
        });
        translations = await response.json();

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
        login();
        return;
    }
    
    const product = mockProducts.find(p => p.id == id);

    if (product) {
        if (!myShelf.some(p => p.id == id)) {
            myShelf.push(product);

            if (buttonElement) {
                buttonElement.classList.add('btn-catalog-added');
                const addedLabel = translations?.AddedStatus || "Added";
                buttonElement.innerHTML = `<i class="fas fa-check"></i> ${addedLabel}`;
            }

            renderFullShelf();
        }
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
    if (source === 'shelf' && !isLogged) {
        login();
        return;
    }
    
    if (source === 'shelf' && myShelf.length === 0) {
        const emptyMsg = translations?.EmptyShelfAlert;
        showAlert(emptyMsg);
        return;
    }
    
    selectedSource = source;

    const btnAll = document.getElementById('btnAll');
    const btnShelf = document.getElementById('btnShelf');

    if(btnAll) btnAll.classList.toggle('active', source === 'all');
    if(btnShelf) btnShelf.classList.toggle('active', source === 'shelf');

    const searchInput = document.getElementById('constructorSearch');
    if (searchInput) searchInput.value = '';

    if (source === 'shelf') {
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
            const msg = translations?.AlreadyInQueue || "Already in queue!";
            showAlert(msg);
        }
    }
}

// Оновлює візуальний список обраних продуктів у конструкторі
function updateAnalysisUI() {
    const container = document.getElementById('queueList');
    const titleText = translations?.ClickToRemove;

    container.innerHTML = analysisQueue.map((p, index) => {
        const iconData = getProductIcon(p);
        return `
            <div class="product-card-mini" onclick="removeFromAnalysis(${index})" title="${titleText}">
                <div class="product-img-stub">
                    <i class="fas ${iconData.icon}" style="color: ${iconData.color};"></i>
                </div>
                <div style="display: flex; flex-direction: column;">
                    <span style="font-weight: 600; color: var(--burgundy); font-size: 0.9rem;">${p.name}</span>
                    <small style="color: var(--lapis-lazuli); font-size: 0.7rem;">${p.type}</small>
                </div>
            </div>
        `;
    }).join('');
}

// Видаляє продукт із черги на аналіз по кліку
function removeFromAnalysis(index) {
    analysisQueue.splice(index, 1);
    updateAnalysisUI();
}

// Відправляє зібрану рутину на бекенд для перевірки сумісності
async function runAnalysis() {
    if (analysisQueue.length < 2) {
        showAlert(translations?.MinProductsAlert);
        return;
    }

    const reportArea = document.getElementById('reportArea');
    reportArea.style.display = 'block';
    
    const loadingMsg = translations?.LoadingText;
    reportArea.innerHTML = `<em>${loadingMsg}</em>`;

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

        let cleanAnalysis = data.analysis
            .replace(/--- РЕЗУЛЬТАТ АНАЛІЗУ ---/gi, '')
            .replace(/--- ANALYSIS RESULT ---/gi, '')
            .trim();

        const hasCriticalConflict = cleanAnalysis.toLowerCase().includes("критичні конфлікти") ||
            cleanAnalysis.toLowerCase().includes("critical conflict");

        let formattedText = cleanAnalysis.replace(/\n/g, '<br>');
        formattedText = formattedText.replace(/(<br>\s*){3,}/g, '<br><br>');
        
        let htmlContent = `
            <div class="analysis-result-header" data-i18n="RoutineHeader" style="text-align: center !important; margin-bottom: 15px; color: var(--misty-rose) !important; font-family: 'Playfair Display', serif !important;">РЕЗУЛЬТАТ АНАЛІЗУ</div>
            <div class="analysis-main-msg" style="text-align: center !important; font-weight: normal !important; line-height: 1.8 !important; color: var(--misty-rose) !important;">
                ${formattedText}
            </div>
        `;

        if (hasCriticalConflict) {
            const warningMsg = translations?.RoutineNotBuilt || "Routine not built due to conflicts.";
            
            htmlContent += `
                <div style="margin-top: 20px; padding: 20px; background-color: rgba(242, 174, 188, 0.15) !important; border: 1px solid rgba(242, 174, 188, 0.3) !important; border-radius: 12px; color: var(--misty-rose) !important; text-align: center !important;">
                    <i class="fas fa-exclamation-triangle" style="font-size: 1.5rem; color: var(--cherry-pink); margin-bottom: 10px; opacity: 0.8;"></i><br>
                    <div style="font-size: 0.95rem !important; line-height: 1.5 !important; font-weight: normal !important; font-family: inherit !important;">
                        ${warningMsg}
                    </div>
                </div>
            `;
        } else {
            const routine = getRoutineSuggestion();
            
            const morningTitle = translations?.MorningTitle;
            const eveningTitle = translations?.EveningTitle;

            htmlContent += `
                <div class="routine-recommendation">
                    <div class="routine-block">
                        <p><i class="fas fa-sun" style="color: #f1c40f;"></i> <strong>${morningTitle}</strong></p>
                        <p class="routine-path">${routine.morning.length > 0 ? routine.morning.map(p => p.name).join(' → ') : '---'}</p>
                    </div>
                    <div class="routine-block" style="margin-top: 15px;">
                        <p><i class="fas fa-moon" style="color: #bdc3c7;"></i> <strong>${eveningTitle}</strong></p>
                        <p class="routine-path">${routine.evening.length > 0 ? routine.evening.map(p => p.name).join(' → ') : '---'}</p>
                    </div>
                </div>
            `;
        }

        reportArea.innerHTML = htmlContent;
        await applyTranslations();

    } catch (e) {
        console.error(e);
        const errorMsg = translations?.ServerError;
        reportArea.innerText = errorMsg;
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
    userName = localStorage.getItem('userName') || "";
    isLogged = !!userName;
    
    if (isLogged) {
        updateUIForLoggedInUser();
    }
    
    await applyTranslations();
    
    try {
        const response = await fetch('http://localhost:5016/api/skincare/products');
        mockProducts = await response.json();

        setSource('all');
        console.log(`Завантажено ${mockProducts.length} реальних продуктів`);
    } catch (e) {
        console.error("Помилка завантаження бази продуктів:", e);
    }
};

function updateUIForLoggedInUser() {
    const btn = document.querySelector('.auth-btn');
    const logoutBtn = document.getElementById('logoutBtn');

    if (btn && userName) {
        btn.innerHTML = `<i class="fas fa-user"></i> ${userName}`;
        btn.onclick = null;
    }

    if (logoutBtn) {
        logoutBtn.style.display = 'inline-block';
    }
}

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
    const name = document.getElementById('loginName').value.trim();
    const pass = document.getElementById('loginPass').value.trim();

    if (!name || !pass) {
        showAlert(translations?.FillAllFields || "Заповніть всі поля!");
        return;
    }
    
    let users = JSON.parse(localStorage.getItem('registeredUsers') || "{}");

    if (users[name]) {
        if (users[name] === pass) {
            loginSuccess(name);
        } else {
            showAlert(translations?.WrongPass || "Невірний пароль!");
        }
    } else {
        users[name] = pass;
        localStorage.setItem('registeredUsers', JSON.stringify(users));
        loginSuccess(name);
        showAlert(translations?.RegisteredSuccess || "Ви успішно зареєстровані!");
    }
}

function loginSuccess(name) {
    userName = name;
    isLogged = true;
    localStorage.setItem('userName', name);

    updateUIForLoggedInUser();
    closeLogin();

    const welcome = (translations?.WelcomeUser || "Вітаємо, {name}!").replace('{name}', userName);
    showAlert(welcome);
}

function logout() {
    isLogged = false;
    userName = "";
    localStorage.removeItem('userName');
    location.reload();
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
    
    const btnText = translations?.BtnToShelf;
    const addedText = translations?.AddedStatus;
    const nothingFoundText = translations?.NothingFound;

    let filteredProducts = mockProducts.filter(p =>
        p.name.toLowerCase().includes(searchTerm) ||
        (p.brand && p.brand.toLowerCase().includes(searchTerm)) ||
        p.type.toLowerCase().includes(searchTerm)
    );

    if (filteredProducts.length === 0) {
        container.style.display = 'block';
        container.innerHTML = `<div style="text-align: center; padding: 40px; color: var(--lapis-lazuli);"><p>${nothingFoundText}</p></div>`;
        return;
    }
    
    const totalPages = Math.ceil(filteredProducts.length / itemsPerPage);
    if (catalogPage > totalPages) catalogPage = 1;

    const start = (catalogPage - 1) * itemsPerPage;
    const displayProducts = filteredProducts.slice(start, start + itemsPerPage);

    container.style.display = 'grid';
    container.innerHTML = displayProducts.map(p => {
        const alreadyInShelf = myShelf.some(item => item.id === p.id);
        const btnClass = alreadyInShelf ? "btn-catalog-add btn-catalog-added" : "btn-catalog-add";
        
        const iconData = getProductIcon(p);

        return `
        <div class="catalog-card">
            <div class="catalog-img">
                <i class="fas ${iconData.icon}" style="color: ${iconData.color};"></i>
            </div>
            <h4>${p.name}</h4>
            <small>${p.brand || 'Skincare'} • ${p.type}</small>
            <button class="${btnClass}" onclick="addToShelf('${p.id}', this)">
                <i class="fas ${alreadyInShelf ? "fa-check" : "fa-plus"}"></i> 
                ${alreadyInShelf ? addedText : btnText}
            </button>
        </div>`;
    }).join('');

    renderPaginationUI(totalPages);
}

function renderPaginationUI(totalPages) {
    const catalogSection = document.getElementById('catalogSection');
    let nav = document.getElementById('catalogPagination');

    if (!nav) {
        nav = document.createElement('div');
        nav.id = 'catalogPagination';
        nav.className = 'pagination-container';
        catalogSection.appendChild(nav);
    }

    if (totalPages <= 1) {
        nav.innerHTML = '';
        return;
    }

    nav.innerHTML = `
        <button onclick="changeCatalogPage(-1)" ${catalogPage === 1 ? 'disabled' : ''}><i class="fas fa-chevron-left"></i></button>
        <span>${catalogPage} / ${totalPages}</span>
        <button onclick="changeCatalogPage(1)" ${catalogPage === totalPages ? 'disabled' : ''}><i class="fas fa-chevron-right"></i></button>
    `;
}

function changeCatalogPage(step) {
    catalogPage += step;
    renderCatalog();
    window.scrollTo({ top: 0, behavior: 'smooth' });
}

function renderFullShelf() {
    const container = document.getElementById('fullShelfDisplay');
    const userTitle = document.getElementById('shelfUserName');
    if (!container) return;

    const t = translations;
    const title = t?.CollectionTitle || "Колекція";
    userTitle.innerText = `${title}: ${userName}`;

    if (myShelf.length === 0) {
        const emptyText = translations?.EmptyShelfText;
        container.innerHTML = `
            <div style="grid-column: 1/-1; text-align: center; padding: 40px;">
                <i class="fas fa-ghost" style="font-size: 3rem; color: #ccc; margin-bottom: 15px;"></i>
                <p>${emptyText}</p>
            </div>`;
        return;
    }

    container.innerHTML = myShelf.map((p, index) => {
        let timeLabel = translations?.TimeBoth || "Anytime";
        if (p.time === 'morning') timeLabel = translations?.TimeMorning || "Morning";
        if (p.time === 'evening') timeLabel = translations?.TimeEvening || "Evening";

        const removeText = translations?.RemoveFromShelf;
        
        const iconData = getProductIcon(p);

        return `
            <div class="catalog-card">
                <button class="btn-remove-shelf" onclick="removeFromShelf(${index})" title="${removeText}">
                    <i class="fas fa-times"></i>
                </button>
                
                <div class="catalog-img">
                    <i class="fas ${iconData.icon}" style="color: ${iconData.color};"></i>
                </div>
                <h4>${p.name}</h4>
                <small style="color: var(--lapis-lazuli);">${p.type}</small>
                
                <div style="font-size: 0.75rem; color: var(--silver-blue); margin-top: 10px; display: flex; align-items: center; justify-content: center; gap: 5px;">
                    <i class="far fa-clock"></i> <span>${timeLabel}</span>
                </div>
            </div>
        `;
    }).join('');
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
        container.innerHTML = '';
        return;
    }

    const filtered = sourceArray.filter(p =>
        p.name.toLowerCase().includes(term) || (p.brand && p.brand.toLowerCase().includes(term))
    );

    if (filtered.length === 0) {
        container.innerHTML = `<div style="text-align:center; padding: 10px;">${translations?.NothingFound}</div>`;
        return;
    }
    
    const displayLimit = filtered.slice(0, 100);
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
    container.innerHTML = list.map(p => {
        const iconData = getProductIcon(p);
        return `
        <div class="constructor-result-item" onclick="addProductToAnalysis('${p.id}')">
            <div style="display: flex; align-items: center; gap: 10px;">
                <i class="fas ${iconData.icon}" style="color: ${iconData.color}; width: 20px; text-align: center;"></i>
                <div>
                    <strong style="color:var(--burgundy); font-size:0.9rem;">${p.name}</strong><br>
                    <small style="color:var(--lapis-lazuli);">${p.brand || 'Skincare'} • ${p.type}</small>
                </div>
            </div>
            <i class="fas fa-plus" style="color:var(--burgundy);"></i>
        </div>
    `;}).join('');
}