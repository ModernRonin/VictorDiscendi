var config = {
    "domain": "oldman.eu.auth0.com",
    "clientId": "PCXHj3vHt1gCjgVkdYwRuUHmQtt8s11v"
};

let auth0 = null;
let isAuthenticated = false;


const configureClient = async () => {
    auth0 = await createAuth0Client({
        domain: config.domain,
        client_id: config.clientId
    });
};

const login = async () => {
    await auth0.loginWithRedirect({
        redirect_uri: window.location.origin
    });
};

const logout = () => {
    auth0.logout({
        returnTo: window.location.origin
    });
};

const updateAuthenticationStatus = async () => {
    isAuthenticated = await auth0.isAuthenticated();
};

const getIsLoggedIn = () => isAuthenticated;

const onLoad = async () => {
    await configureClient();
    await updateAuthenticationStatus();

    if (isAuthenticated) {
        return;
    }

    const query = window.location.search;
    if (query.includes("code=") && query.includes("state=")) {
        await auth0.handleRedirectCallback();
        await updateAuthenticationStatus();
        window.history.replaceState({}, document.title, "/");
    }
};

var AuthJS = {
    login: login,
    logout: logout,
    getIsLoggedIn: getIsLoggedIn,
    onPageLoad: onLoad
};