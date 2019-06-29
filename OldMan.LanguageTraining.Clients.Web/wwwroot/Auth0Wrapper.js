var Auth0Wrapper =
{
     _config: {
        domain: "oldman.eu.auth0.com",
        clientId: "PCXHj3vHt1gCjgVkdYwRuUHmQtt8s11v"
    },

    _auth0: null,

    configureClient : async () => {

        Auth0Wrapper._auth0 = await createAuth0Client({
                    domain: Auth0Wrapper._config.domain,
                    client_id: Auth0Wrapper._config.clientId
                });
    }, 
    updateAuthenticationStateus: async () => {
        Auth0Wrapper.isAuthenticated = await Auth0Wrapper._auth0.isAuthenticated();
    },
    isAuthenticated: false,
    login: async () => {
         await Auth0Wrapper._auth0.loginWithRedirect({
             redirect_uri: window.location.href
         });
    }
}

window.onload = async () => {
    await Auth0Wrapper.configureClient();
    await Auth0Wrapper.updateAuthenticationStateus();
}

