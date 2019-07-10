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
        redirect_uri: window.location.origin + "/#/auth/loggedin"
    });
};

const logout = () => {
    auth0.logout({
        returnTo: window.location.origin + "/#/auth/loggedout"
    });
};

const updateAuthenticationStatus = async () => {
    isAuthenticated = await auth0.isAuthenticated();
};

const getIsLoggedIn = () => isAuthenticated;

const onLoad = async () => {
    console.log("onLoad");
    try {
        console.log("configuring");
        await configureClient();
        console.log("updating");
        await updateAuthenticationStatus();
        console.log(auth0);
    } catch (e) {
        console.error(e);
    }

    if (isAuthenticated) {
        return;
    }

    const query = window.location.search;
    if (query.includes("code=") && query.includes("state=")) {
        try {

            await auth0.handleRedirectCallback();
            await updateAuthenticationStatus();
        } catch (e) {
            console.error(e);
        }

        window.history.replaceState({}, document.title, "/#/auth/loggedin");
    }
};

var AuthJS = {
    login: login,
    logout: logout,
    getIsLoggedIn: getIsLoggedIn,
    onPageLoad: onLoad
};