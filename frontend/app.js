// ============================================================
// HabitTo - API Client Layer
// Base URL: ajusta si corres la API en otro puerto
// ============================================================
const API_BASE = 'http://localhost:8080';

// ============================================================
// Imágenes y datos visuales de enriquecimiento (el backend
// no almacena imágenes todavía, se complementan aquí por título)
// ============================================================
const PROPERTY_VISUALS = {
    default: {
        images: [
            'https://images.unsplash.com/photo-1564013799919-ab600027ffc6?auto=format&fit=crop&w=1200&q=80',
            'https://images.unsplash.com/photo-1580587771525-78b9dba3b914?auto=format&fit=crop&w=600&q=80',
            'https://images.unsplash.com/photo-1512917774080-9991f1c4c750?auto=format&fit=crop&w=600&q=80',
        ],
        description: 'Premium accommodation offering high-end amenities in a breathtaking location. Carefully designed for comfort and relaxation.',
        amenities: ['Wifi', 'AC', 'Kitchen', 'Parking'],
        features: ['2 Guests', '1 Bedroom', '1 Bed', '1 Bath'],
        category: 'Apartments',
        owner: { fullName: 'HabitTo Host', avatar: 'https://images.unsplash.com/photo-1535713875002-d1d0cf377fde?auto=format&fit=crop&w=150&q=80', joined: '2024' }
    }
};

function getVisuals(property) {
    const id = property?.id || property?.Id;
    const stored = id ? JSON.parse(localStorage.getItem('habitto_property_visuals') || '{}')[id] : {};
    return { ...PROPERTY_VISUALS.default, ...stored, ...property };
}

function savePropertyVisuals(propertyId, visuals) {
    const current = JSON.parse(localStorage.getItem('habitto_property_visuals') || '{}');
    current[propertyId] = { ...(current[propertyId] || {}), ...visuals };
    localStorage.setItem('habitto_property_visuals', JSON.stringify(current));
}

// ============================================================
// Token helpers
// ============================================================
const Auth = {
    getToken: () => localStorage.getItem('habitto_token'),
    getUserId: () => localStorage.getItem('habitto_user_id'),
    getUser: () => {
        const raw = localStorage.getItem('habitto_user');
        return raw ? JSON.parse(raw) : null;
    },
    setSession: (token, userId, userObj) => {
        localStorage.setItem('habitto_token', token);
        localStorage.setItem('habitto_user_id', userId);
        localStorage.setItem('habitto_user', JSON.stringify(userObj));
    },
    clearSession: () => {
        localStorage.removeItem('habitto_token');
        localStorage.removeItem('habitto_user_id');
        localStorage.removeItem('habitto_user');
    },
    isLoggedIn: () => !!localStorage.getItem('habitto_token'),
};

// ============================================================
// HTTP helper — añade Authorization header si hay token
// ============================================================
async function apiFetch(path, options = {}) {
    const token = Auth.getToken();
    const headers = {
        'Content-Type': 'application/json',
        ...(token ? { 'Authorization': `Bearer ${token}` } : {}),
        ...(options.headers || {}),
    };
    const response = await fetch(`${API_BASE}${path}`, { ...options, headers });

    if (response.status === 401) {
        Auth.clearSession();
        HabittoUI.toggleAuthModal(true, 'login');
        throw new Error('Unauthorized – please log in.');
    }

    return response;
}

// ============================================================
// API calls
// ============================================================
window.HabittoAPI = {

    // --- AUTH ---
    register: async (email, password, fullName) => {
        const res = await apiFetch('/api/auth/register', {
            method: 'POST',
            body: JSON.stringify({ email, password, fullName })
        });
        if (!res.ok) {
            const text = await res.text();
            return { success: false, message: text || 'Registration failed.' };
        }
        const data = await res.json();
        // Store minimal user info locally for UI display
        Auth.setSession(data.token, data.userId, { id: data.userId, email, fullName });
        return { success: true, data };
    },

    login: async (email, password) => {
        const res = await apiFetch('/api/auth/login', {
            method: 'POST',
            body: JSON.stringify({ email, password })
        });
        if (!res.ok) {
            const text = await res.text();
            return { success: false, message: text || 'Invalid credentials.' };
        }
        const data = await res.json();
        Auth.setSession(data.token, data.userId, { id: data.userId, email, fullName: email });
        return { success: true, data };
    },

    logout: () => {
        Auth.clearSession();
        window.location.href = 'home.html';
    },

    // --- PROPERTIES ---
    getProperties: async ({ city, from, to } = {}) => {
        let qs = new URLSearchParams();
        if (city) qs.set('city', city);
        if (from) qs.set('from', from);
        if (to) qs.set('to', to);
        const res = await apiFetch(`/api/properties?${qs.toString()}`);
        if (!res.ok) throw new Error('Failed to load properties.');
        const list = await res.json();
        // Enrich with visual data
        return list.map(p => ({ ...getVisuals(p), ...p }));
    },

    getProperty: async (id) => {
        const res = await apiFetch(`/api/properties/${id}`);
        if (res.status === 404) return null;
        if (!res.ok) throw new Error('Failed to load property.');
        const p = await res.json();
        return { ...getVisuals(p), ...p };
    },

    createProperty: async (body) => {
        const payload = {
            ownerId: body.ownerId,
            title: body.title,
            city: body.city,
            latitude: body.latitude ?? 0,
            longitude: body.longitude ?? 0,
            nightlyRate: body.nightlyRate
        };
        const res = await apiFetch('/api/properties', {
            method: 'POST',
            body: JSON.stringify(payload)
        });
        if (!res.ok) {
            const text = await res.text();
            throw new Error(text || 'Failed to create property.');
        }
        const created = await res.json();
        savePropertyVisuals(created.id, {
            category: body.category || 'Apartments',
            description: body.description,
            features: body.features,
            amenities: body.amenities,
            images: body.images,
            owner: body.owner
        });
        return { ...getVisuals(created), ...created };
    },

    // --- BOOKINGS ---
    createBooking: async ({ userId, propertyId, checkIn, checkOut }) => {
        const res = await apiFetch('/api/bookings', {
            method: 'POST',
            body: JSON.stringify({
                userId,
                propertyId,
                checkIn,   // "YYYY-MM-DD"
                checkOut   // "YYYY-MM-DD"
            })
        });
        if (!res.ok) {
            const text = await res.text();
            throw new Error(text || 'Booking failed.');
        }
        return await res.json(); // { bookingId, totalPrice, checkInUtc, checkOutUtc }
    },

    getBookings: async (userId) => {
        const res = await apiFetch(`/api/bookings/user/${userId}`);
        if (!res.ok) throw new Error('Failed to load bookings.');
        return await res.json();
    },

    // --- WISHLIST ---
    addToWishlist: async (userId, propertyId) => {
        const res = await apiFetch('/api/wishlist', {
            method: 'POST',
            body: JSON.stringify({ userId, propertyId })
        });
        if (!res.ok) throw new Error('Failed to add to wishlist.');
    },

    removeFromWishlist: async (userId, propertyId) => {
        const res = await apiFetch(`/api/wishlist?userId=${userId}&propertyId=${propertyId}`, {
            method: 'DELETE'
        });
        if (!res.ok) throw new Error('Failed to remove from wishlist.');
    },

    getWishlist: async (userId) => {
        const res = await apiFetch(`/api/wishlist/${userId}`);
        if (!res.ok) throw new Error('Failed to load wishlist.');
        return await res.json(); // [ { userId, propertyId } ]
    },

    // --- IDENTITY ---
    verifyIdentity: async (userId, file) => {
        const arrayBuffer = await file.arrayBuffer();
        const bytes = Array.from(new Uint8Array(arrayBuffer));
        const res = await apiFetch('/api/identity/verify', {
            method: 'POST',
            body: JSON.stringify({ userId, documentImage: bytes })
        });
        if (!res.ok) {
            const text = await res.text();
            throw new Error(text || 'Identity verification failed.');
        }
        return await res.json();
    },

    // --- REPORTS ---
    getReport: async (ownerId) => {
        const res = await apiFetch(`/api/reports/owner/${ownerId}`);
        if (!res.ok) throw new Error('Failed to load report.');
        return await res.json();
    },

    exportReportExcel: (ownerId) => {
        // Opens download link directly; token in URL not ideal but works for demo
        const token = Auth.getToken();
        const url = `${API_BASE}/api/reports/owner/${ownerId}/excel`;
        // Use fetch to download as blob
        apiFetch(`/api/reports/owner/${ownerId}/excel`)
            .then(res => res.blob())
            .then(blob => {
                const a = document.createElement('a');
                a.href = URL.createObjectURL(blob);
                a.download = `reporte-reservas-${new Date().toISOString().slice(0,10)}.xlsx`;
                a.click();
            })
            .catch(() => alert('Could not generate Excel report.'));
    }
};

// ============================================================
// UI Helpers: Modal, Navbar, Theme
// ============================================================
window.HabittoUI = {

    toggleAuthModal: (show, tab = 'login') => {
        let modal = document.getElementById('loginModal');
        if (!modal) {
            const modalHtml = `
            <div id="loginModal" class="fixed inset-0 z-[100] hidden items-center justify-center bg-black/50 backdrop-blur-sm">
                <div class="bg-surface dark:bg-inverse-surface border border-outline-variant/30 w-full max-w-md rounded-[1.5rem] p-lg shadow-2xl relative">
                    <button class="absolute top-4 right-4 text-on-surface-variant hover:text-primary transition-colors" onclick="HabittoUI.toggleAuthModal(false)">
                        <span class="material-symbols-outlined">close</span>
                    </button>

                    <div class="flex border-b border-outline-variant/20 mb-lg">
                        <button id="auth-tab-login" class="flex-1 pb-sm font-label-md border-b-2 border-primary text-primary transition-colors" onclick="HabittoUI.switchAuthTab('login')">Log In</button>
                        <button id="auth-tab-register" class="flex-1 pb-sm font-label-md border-b-2 border-transparent text-on-surface-variant transition-colors" onclick="HabittoUI.switchAuthTab('register')">Sign Up</button>
                    </div>

                    <div id="auth-alert" class="hidden mb-md p-sm bg-error-container text-on-error-container rounded-lg text-label-sm font-semibold flex items-center gap-xs">
                        <span class="material-symbols-outlined text-[18px]">error</span>
                        <span id="auth-alert-text">Error</span>
                    </div>

                    <form id="login-form" class="space-y-md" onsubmit="HabittoUI.handleAuthSubmit(event,'login')">
                        <div>
                            <label class="block text-[11px] font-bold uppercase tracking-wider text-on-surface-variant mb-xs">Email</label>
                            <input type="email" id="login-email" class="w-full bg-surface-container-low border border-outline-variant rounded-xl px-md py-sm focus:ring-primary focus:border-primary text-on-surface" required placeholder="name@example.com"/>
                        </div>
                        <div>
                            <label class="block text-[11px] font-bold uppercase tracking-wider text-on-surface-variant mb-xs">Password</label>
                            <input type="password" id="login-password" class="w-full bg-surface-container-low border border-outline-variant rounded-xl px-md py-sm focus:ring-primary focus:border-primary text-on-surface" required placeholder="••••••••"/>
                        </div>
                        <button type="submit" class="w-full bg-primary text-on-primary font-bold py-md rounded-xl hover:opacity-90 active:scale-95 transition-all shadow-md">Continue</button>
                    </form>

                    <form id="register-form" class="hidden space-y-md" onsubmit="HabittoUI.handleAuthSubmit(event,'register')">
                        <div>
                            <label class="block text-[11px] font-bold uppercase tracking-wider text-on-surface-variant mb-xs">Full Name</label>
                            <input type="text" id="register-name" class="w-full bg-surface-container-low border border-outline-variant rounded-xl px-md py-sm focus:ring-primary focus:border-primary text-on-surface" required placeholder="John Doe"/>
                        </div>
                        <div>
                            <label class="block text-[11px] font-bold uppercase tracking-wider text-on-surface-variant mb-xs">Email</label>
                            <input type="email" id="register-email" class="w-full bg-surface-container-low border border-outline-variant rounded-xl px-md py-sm focus:ring-primary focus:border-primary text-on-surface" required placeholder="name@example.com"/>
                        </div>
                        <div>
                            <label class="block text-[11px] font-bold uppercase tracking-wider text-on-surface-variant mb-xs">Password</label>
                            <input type="password" id="register-password" class="w-full bg-surface-container-low border border-outline-variant rounded-xl px-md py-sm focus:ring-primary focus:border-primary text-on-surface" required placeholder="••••••••"/>
                        </div>
                        <button type="submit" class="w-full bg-primary text-on-primary font-bold py-md rounded-xl hover:opacity-90 active:scale-95 transition-all shadow-md">Create Account</button>
                    </form>
                </div>
            </div>`;
            document.body.insertAdjacentHTML('beforeend', modalHtml);
            modal = document.getElementById('loginModal');
        }

        if (show) {
            modal.classList.remove('hidden');
            modal.classList.add('flex');
            HabittoUI.switchAuthTab(tab);
        } else {
            modal.classList.add('hidden');
            modal.classList.remove('flex');
        }
    },

    switchAuthTab: (tab) => {
        const loginForm = document.getElementById('login-form');
        const registerForm = document.getElementById('register-form');
        const loginTab = document.getElementById('auth-tab-login');
        const registerTab = document.getElementById('auth-tab-register');
        const alert = document.getElementById('auth-alert');
        if (alert) alert.classList.add('hidden');

        if (tab === 'login') {
            loginForm.classList.remove('hidden');
            registerForm.classList.add('hidden');
            loginTab.className = 'flex-1 pb-sm font-label-md border-b-2 border-primary text-primary transition-colors';
            registerTab.className = 'flex-1 pb-sm font-label-md border-b-2 border-transparent text-on-surface-variant hover:text-primary transition-colors';
        } else {
            loginForm.classList.add('hidden');
            registerForm.classList.remove('hidden');
            registerTab.className = 'flex-1 pb-sm font-label-md border-b-2 border-primary text-primary transition-colors';
            loginTab.className = 'flex-1 pb-sm font-label-md border-b-2 border-transparent text-on-surface-variant hover:text-primary transition-colors';
        }
    },

    handleAuthSubmit: async (event, type) => {
        event.preventDefault();
        const alertEl = document.getElementById('auth-alert');
        const alertText = document.getElementById('auth-alert-text');
        alertEl.classList.add('hidden');

        const submitBtn = event.target.querySelector('button[type="submit"]');
        const originalText = submitBtn.textContent;
        submitBtn.textContent = 'Loading...';
        submitBtn.disabled = true;

        try {
            let result;
            if (type === 'login') {
                const email = document.getElementById('login-email').value;
                const pass = document.getElementById('login-password').value;
                result = await HabittoAPI.login(email, pass);
            } else {
                const name = document.getElementById('register-name').value;
                const email = document.getElementById('register-email').value;
                const pass = document.getElementById('register-password').value;
                result = await HabittoAPI.register(email, pass, name);
            }

            if (result.success) {
                HabittoUI.toggleAuthModal(false);
                window.location.reload();
            } else {
                alertEl.classList.remove('hidden');
                alertText.textContent = result.message;
            }
        } catch (e) {
            alertEl.classList.remove('hidden');
            alertText.textContent = e.message || 'Network error. Is the API running?';
        } finally {
            submitBtn.textContent = originalText;
            submitBtn.disabled = false;
        }
    },

    // User dropdown
    initNavbar: () => {
        const user = Auth.getUser();

        // Theme setup
        const themeBtn = document.getElementById('theme-toggle') || document.getElementById('themeToggle');
        const themeIcon = document.getElementById('theme-icon') || (themeBtn ? themeBtn.querySelector('.material-symbols-outlined') : null);
        const html = document.documentElement;
        const saved = localStorage.getItem('theme') || 'light';

        html.classList.toggle('dark', saved === 'dark');
        html.classList.toggle('light', saved !== 'dark');
        if (themeIcon) themeIcon.textContent = saved === 'dark' ? 'light_mode' : 'dark_mode';

        if (themeBtn) {
            themeBtn.addEventListener('click', (e) => {
                e.stopPropagation();
                const isDark = html.classList.contains('dark');
                html.classList.toggle('dark', !isDark);
                html.classList.toggle('light', isDark);
                localStorage.setItem('theme', isDark ? 'light' : 'dark');
                if (themeIcon) themeIcon.textContent = isDark ? 'dark_mode' : 'light_mode';
            });
        }

        // Attach profile dropdown
        const profileTrigger = document.querySelector('nav .flex.items-center.gap-sm.border.border-outline-variant');
        if (profileTrigger) {
            profileTrigger.style.position = 'relative';
            profileTrigger.addEventListener('click', (e) => {
                e.stopPropagation();
                let dd = document.getElementById('user-dropdown');
                if (!dd) {
                    let items = '';
                    if (user) {
                        items = `
                        <div class="px-md pt-sm pb-xs border-b border-outline-variant/20">
                            <p class="font-semibold text-on-surface text-label-md truncate">${user.fullName || user.email}</p>
                            <p class="text-[11px] text-on-surface-variant truncate">${user.email}</p>
                        </div>
                        <a href="trips.html" class="block px-md py-sm text-label-md text-on-surface hover:bg-surface-container-high transition-colors">My Bookings</a>
                        <a href="trips.html?tab=favorites" class="block px-md py-sm text-label-md text-on-surface hover:bg-surface-container-high transition-colors">My Favorites</a>
                        <a href="profile.html" class="block px-md py-sm text-label-md text-on-surface hover:bg-surface-container-high transition-colors">Dashboard</a>
                        <div class="h-px bg-outline-variant/30 my-xs"></div>
                        <button onclick="HabittoAPI.logout()" class="w-full text-left px-md py-sm text-label-md text-error hover:bg-surface-container-high transition-colors">Log Out</button>`;
                    } else {
                        items = `
                        <button onclick="HabittoUI.toggleAuthModal(true,'login')" class="w-full text-left px-md py-sm text-label-md text-on-surface font-semibold hover:bg-surface-container-high transition-colors">Log In</button>
                        <button onclick="HabittoUI.toggleAuthModal(true,'register')" class="w-full text-left px-md py-sm text-label-md text-on-surface-variant hover:bg-surface-container-high transition-colors">Sign Up</button>`;
                    }
                    profileTrigger.insertAdjacentHTML('beforeend', `
                        <div id="user-dropdown" class="absolute right-0 top-full mt-2 w-56 bg-surface border border-outline-variant/30 rounded-xl shadow-xl py-sm z-[90]">
                            ${items}
                        </div>`);
                    dd = document.getElementById('user-dropdown');
                } else {
                    dd.remove();
                }
            });
        }

        // Close dropdown on outside click
        window.addEventListener('click', () => {
            const dd = document.getElementById('user-dropdown');
            if (dd) dd.remove();
        });
    }
};

// ============================================================
// Auto-init
// ============================================================
window.addEventListener('DOMContentLoaded', () => {
    HabittoUI.toggleAuthModal(false); // pre-create modal DOM
    HabittoUI.initNavbar();
});
