using UnityEngine;
using System.Collections;
//-----------------------------------------------------------
//Enum defining all possible game events
//More events should be added to the list
// Enums have been modified from the Thorn example 
// to make sense with this scene.
public enum EVENT_TYPE {GAME_INIT, 
						GAME_END,
						TARGET_FOUND
						};
//-----------------------------------------------------------
//Listener interface to be implemented on Listener classes
public interface IListener
{
	//Notification function to be invoked on Listeners when events happen
	void OnEvent(EVENT_TYPE Event_Type, Component Sender, object Param = null);
}
//-----------------------------------------------------------