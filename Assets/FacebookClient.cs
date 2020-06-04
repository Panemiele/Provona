using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Facebook.Unity;

public class FacebookClient : MonoBehaviour {
	public GameObject NotLoggedInUI;
	public GameObject LoggedInUI;
	public bool isLoggedIn;
    private string FbFirstName;

    void Awake(){
		FB.Init (InitCallBack);
	}

	void InitCallBack(){
		ShowUI ();
	}

	public void Login(){
		FB.LogInWithReadPermissions (new List<string>{"public_profile", "email"}, LoginCallBack);
	}

	void LoginCallBack(IResult result){
		if (string.IsNullOrEmpty(result.Error))
			ShowUI ();
		else
			Debug.Log ("Error during login : " + result.Error);
	}

	public void Logout(){
		FB.LogOut();
		ShowUI ();
	}

	void ShowUI(){
		if (FB.IsLoggedIn) {
			NotLoggedInUI.SetActive (false);
			LoggedInUI.SetActive (true);
			this.GetComponent<SyncClient>().FBHasLoggedIn(AccessToken.CurrentAccessToken.TokenString, AccessToken.CurrentAccessToken.UserId);
			FB.API ("me/picture?width=100&height=100", HttpMethod.GET, PictureCallBack);
			FB.API ("me?fields=first_name", HttpMethod.GET, NameCallBack);
			isLoggedIn = true;
		} else {
			NotLoggedInUI.SetActive (true);
			LoggedInUI.SetActive (false);
			isLoggedIn = false;
		}
	}

	void PictureCallBack(IGraphResult result){
		LoggedInUI.transform.Find("Image").GetComponentInChildren<Image> ().sprite = Sprite.Create (result.Texture, new Rect (0, 0, 100, 100), new Vector2 (0.5f, 0.5f));
	}

	void NameCallBack(IGraphResult result){
		IDictionary<string,object>  profil = result.ResultDictionary;
		FbFirstName = profil["first_name"].ToString();
		Debug.Log("Ecco il nome: " + FbFirstName);
		LoggedInUI.transform.Find("Name").GetComponent<Text>().text = "Benvenuto " + profil["first_name"].ToString() + "!";
	}

    public string GetFbFirstName()
    {
		return FbFirstName;
    }
}
