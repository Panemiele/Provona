using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

using Amazon;
using Amazon.Runtime;
using Amazon.CognitoIdentity;
using Amazon.CognitoIdentity.Model;
using Amazon.CognitoSync;
using Amazon.CognitoSync.SyncManager;

public class SyncClient : MonoBehaviour {
	public GameObject LoggedInUI;
	bool sync = false;
	string name;
	int health;
	int exp;
	Dataset playerInfo;
	CognitoSyncManager syncManager;
	CognitoAWSCredentials credentials;

	void Start () {
		UnityInitializer.AttachToGameObject (this.gameObject);
		//Remove if you want to build on an IOS device.
		AWSConfigs.LoggingConfig.LogTo = LoggingOptions.UnityLogger;
		AWSConfigs.HttpClient = AWSConfigs.HttpClientOption.UnityWebRequest;
		credentials = new CognitoAWSCredentials ("eu-central-1:a3ee931a-4748-432f-be7e-3446d3f370b0",
                                                 RegionEndpoint.EUCentral1);
		syncManager = new CognitoSyncManager (credentials, RegionEndpoint.EUCentral1);
		playerInfo = syncManager.OpenOrCreateDataset ("playerInfo");
		playerInfo.OnSyncSuccess += SyncSuccessCallback;
	}

	public void ChangeName(string newName){
		name = newName;
		playerInfo.Put ("name", newName);
	}

	public void ChangeHealth(string newHealth){
		try{
			health = int.Parse(newHealth);
			playerInfo.Put("health", newHealth);
		} catch{
		}
	}
	
	public void ChangeExp(string newExp){
		try{
			exp = int.Parse(newExp);
			playerInfo.Put("exp", newExp);
		} catch{
		}
	}

	public void Synchronize(){
		if (!string.IsNullOrEmpty (playerInfo.Get ("FacebookId")) && !this.GetComponent<FacebookClient> ().isLoggedIn) {
			Debug.Log ("You must logged in to synchronize.");
		} else {
			sync = true;
			playerInfo.SynchronizeOnConnectivity ();
		}
		UpdateInformation();
	}

	void UpdateInformation(){
		if (!string.IsNullOrEmpty (playerInfo.Get ("name"))) {
			name = playerInfo.Get("name");
			LoggedInUI.transform.Find ("NameInputField").GetComponent<InputField> ().text = name;
		} else
			LoggedInUI.transform.Find ("NameInputField").GetComponent<InputField> ().text = "";
		if (!string.IsNullOrEmpty (playerInfo.Get ("health"))) {
			health = int.Parse(playerInfo.Get ("health"));
			LoggedInUI.transform.Find ("HealthInputField").GetComponent<InputField> ().text = health.ToString();
		} else
			LoggedInUI.transform.Find ("HealthInputField").GetComponent<InputField> ().text = "";
		if (!string.IsNullOrEmpty (playerInfo.Get ("exp"))) {
			exp = int.Parse (playerInfo.Get ("exp"));
			LoggedInUI.transform.Find ("ExpInputField").GetComponent<InputField> ().text = exp.ToString ();
		} else
			LoggedInUI.transform.Find ("ExpInputField").GetComponent<InputField> ().text = "";
	}

	void SyncSuccessCallback(object sender, SyncSuccessEventArgs e){
		List<Record> newRecords = e.UpdatedRecords;
		for (int k = 0; k < newRecords.Count; k++) {
			Debug.Log (newRecords [k].Key + " was updated: " + newRecords [k].Value);
		}
		UpdateInformation ();
		sync = false;
	}


	public void FBHasLoggedIn(string token, string id) {
		string oldFacebookId = playerInfo.Get ("FacebookId");
		if (string.IsNullOrEmpty(oldFacebookId) || id.Equals (oldFacebookId)) {
			playerInfo.Put ("FacebookId", id);
			playerInfo.Put("name", GetComponent<FacebookClient>().GetFbFirstName());
			credentials.AddLogin ("graph.facebook.com", token);
		} else {
			Debug.Log ("New user detected.");
			credentials.Clear ();
			playerInfo.Delete ();
			credentials.AddLogin ("graph.facebook.com", token);
			Synchronize ();
			StartCoroutine (WaitForEndOfSync (id));
		}
	}

	IEnumerator WaitForEndOfSync(string id){
		while (sync)
			yield return null;
		playerInfo.Put ("FacebookId", id);
	}

}
