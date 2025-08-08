import { jwtDecode } from "jwt-decode";

function isJwtExpired(token : string) {
  try {
    console.log(token)
    const decoded = jwtDecode(token);
    if (!decoded.exp) {
      // No expiration claim, consider it not expired
      return false;
    }
    // exp is in seconds since epoch
    const expiryTime = decoded.exp * 1000;
    return Date.now() > expiryTime;
  } catch (e) {
    // Invalid token format or decoding error
    console.error('Invalid JWT token:', e);
    return true; // Treat invalid token as expired
  }
}

export const GetAuthToken = async () =>
{
    return localStorage.getItem("authToken");
}

export const GetUserProfile = async () =>
{
    var profileData = localStorage.getItem("userProfile");

    if (profileData != null)
    {
        return JSON.parse(profileData)
    }

    return null
    
}

export const SetAuthToken = async (token : string) =>
{
    localStorage.setItem("authToken", token);
}

export const SetUserProfile = async (profile : string) =>
{
    localStorage.setItem("userProfile", profile);
}

export const SignIn = async (token : string, profile : string) =>
{
    SetAuthToken(token);
    SetUserProfile(profile);
}

export const SignOut = async () =>
{
    localStorage.removeItem("authToken");
    localStorage.removeItem("userProfile");
}

export const IsSignedIn = async () =>
{
    var token = await GetAuthToken();

    var tokenExpired = false;

    if (token != null)
    {
        tokenExpired = isJwtExpired(token);
    }

    if (token != null && !tokenExpired)
    {
        return true;
    }

    return false;
}