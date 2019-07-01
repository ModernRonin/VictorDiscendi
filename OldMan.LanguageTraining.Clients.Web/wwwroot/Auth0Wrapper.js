var Auth0Wrapper =
{
    isAuthenticated: false
}

let auth0 = null;
const configureClient = async () => {
        const config= {
            domain: "oldman.eu.auth0.com", 
            clientId: "PCXHj3vHt1gCjgVkdYwRuUHmQtt8s11v"
        }
    auth0= await createAuth0Client({        
        domain: config.domain,
        client_id: config.clientId
    });
};

const updateAuthenticationState = async () => {
    Auth0Wrapper.isAuthenticated = await auth0.isAuthenticated();

};

const login = async ()=> {
    await auth0.loginWithRedirect({
        redirect_uri: window.location.href
    });
}
const logout = () => {
    auth0.logout({
        returnTo: window.location.origin
    });
};
//window.onload = async () => {
//    console.log("Configuring client");
//    await configureClient();
//    console.log("Updating auth state");
//    updateAuthenticationState();    
//    console.log("Setting login/logout functions");
//    Auth0Wrapper.login = login;
//    Auth0Wrapper.logout = logout;
    
//    if (!Auth0Wrapper.isAuthenticated) {
//        const query = window.location.search;
//        if (query.includes("code=") && query.includes("state=")) {
//            await Auth0Wrapper.auth0.handleRedirectCallback();
//            updateAuthenticationState();
//            window.history.replaceState({}, document.title, "/");
//        }
//    }

//}
