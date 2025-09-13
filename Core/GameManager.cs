using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using MyGameNamespace;
public class GameManager:MonoBehaviour{
 public static GameManager Instance{get;private set;}
 public bool verboseLogging=true;
 public StoryManager StoryManager=>StoryManager.Instance;
 void Awake(){ if(Instance!=null&&Instance!=this){Destroy(gameObject);return;} Instance=this; DontDestroyOnLoad(gameObject); }
 void Start(){ GameEventSystem.GetOrCreate(); }
 public void StartNewGame(){ if(verboseLogging) Debug.Log("[GameManager] StartNewGame()"); }
 public void StartNewGame(string playerName,int strength,int dexterity,int constitution,int intelligence,int wisdom,int charisma,string backgroundId,string raceId){
   if(verboseLogging) Debug.Log($"[GameManager] StartNewGame params: {playerName}/{backgroundId}/{raceId}");
   try{ SceneManager.LoadScene("Game",LoadSceneMode.Single);}catch(Exception ex){ Debug.LogError(ex);} }
 public PlayerCharacter GetPlayer(){ return null; }
 public void OnStoryChoiceMade(StoryChoice choice){
   try{ GameEventSystem.Instance?.RaiseStoryChoiceMade(choice);}catch{}
   var target=!string.IsNullOrEmpty(choice?.targetNodeId)?choice.targetNodeId:!string.IsNullOrEmpty(choice?.nextNodeId)?choice.nextNodeId:choice?.nextId;
   if(!string.IsNullOrEmpty(target)){ try{ StoryManager.Instance?.GoToNode(target);}catch{} }
 } }
