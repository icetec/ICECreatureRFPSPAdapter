// ##############################################################################
//
// ICECreatureRFPSPAdapterEditor.cs
// Version 1.1.21
//
// © Pit Vetterick, ICE Technologies Consulting LTD. All Rights Reserved.
// http://www.ice-technologies.com
// mailto:support@ice-technologies.com
// 
// Unity Asset Store End User License Agreement (EULA)
// http://unity3d.com/legal/as_terms
//
// ##############################################################################

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.AnimatedValues;
using ICE;
using ICE.Styles;
using ICE.Layouts;
using ICE.Creatures;
using ICE.Creatures.Components;
using ICE.Creatures.EditorHandler;

namespace ICE.Creatures.Adapter
{
	
	[CustomEditor(typeof(ICECreatureRFPSPAdapter))]
	public class ICECreatureRFPSPAdapterEditor : Editor
	{
		private string _damage_behaviour;
		private DamageType _damage_type;
		public override void OnInspectorGUI()
		{
			ICECreatureRFPSPAdapter _adapter = (ICECreatureRFPSPAdapter)target;
			ICECreatureControl _control = _adapter.GetComponent<ICECreatureControl>();	

			EditorGUILayout.Separator();	

			_adapter.UseCreatureDamage = ICEEditorLayout.ToggleLeft( "Creature Damage", "", _adapter.UseCreatureDamage, true );
			if( _adapter.UseCreatureDamage )
			{
				EditorGUI.indentLevel++;
					_adapter.UseMultipleCreatureDamageHandler = ICEEditorLayout.Toggle( "Use Multiple Damage Handler", "", _adapter.UseMultipleCreatureDamageHandler, "" );
					if( _adapter.UseMultipleCreatureDamageHandler )
					{
						foreach( ICECreatureRFPSPCreatureDamageObject _damage in _adapter.CreatureDamages )
						{
							ICEEditorLayout.BeginHorizontal();
							ICEEditorLayout.Label( _damage.Type.ToString() , true );
							GUILayout.FlexibleSpace();
							if( GUILayout.Button( new GUIContent( "X", "Delete" ), ICEEditorStyle.CMDButton ) )
							{
								_adapter.CreatureDamages.Remove( _damage );
								return;
							}
							ICEEditorLayout.EndHorizontal();
							
							EditorGUI.indentLevel++;
							_damage.BehaviourModeKey = EditorBehaviour.BehaviourSelect( _control, "Behaviour", "", _damage.BehaviourModeKey , "IMPACT" );
							
							_damage.UseAdvanced = ICEEditorLayout.Toggle( "Advanced Influences", "", _damage.UseAdvanced, "" );
							if( _damage.UseAdvanced )
							{
								EditorGUI.indentLevel++;			
								_damage.InfluenceDamage = ICEEditorLayout.DefaultSlider( "Damage", "", _damage.InfluenceDamage, 0.05f, -100, 100, 0 );		
								_damage.InfluenceStress = ICEEditorLayout.DefaultSlider( "Stress", "", _damage.InfluenceStress, 0.05f, -100, 100, 0 );
								_damage.InfluenceDebility = ICEEditorLayout.DefaultSlider( "Debility", "", _damage.InfluenceDebility, 0.05f, -100, 100, 0 );
								_damage.InfluenceHunger = ICEEditorLayout.DefaultSlider( "Hunger", "", _damage.InfluenceHunger, 0.05f, -100, 100, 0 );			
								_damage.InfluenceThirst = ICEEditorLayout.DefaultSlider( "Thirst", "", _damage.InfluenceThirst, 0.05f, -100, 100, 0 );				
								EditorGUI.indentLevel--;
							}
							EditorGUI.indentLevel--;
							
						}
						ICEEditorStyle.SplitterByIndent(EditorGUI.indentLevel + 1);
						ICEEditorLayout.BeginHorizontal();
						_damage_type = (DamageType)ICEEditorLayout.EnumPopup( "Add Damage Handler by Type", "", _damage_type );				
						if( GUILayout.Button( new GUIContent( "ADD", "Adds a new damage handler" ), ICEEditorStyle.CMDButtonDouble ) )
							_adapter.AddCreatureDamage( _damage_type );
						ICEEditorLayout.EndHorizontal();
						
					}
					else
					{
						_adapter.BehaviourModeKey = EditorBehaviour.BehaviourSelect( _control, "Behaviour", "", _adapter.BehaviourModeKey , "IMPACT" );
						_adapter.UseAdvanced = ICEEditorLayout.Toggle( "Advanced Influences", "", _adapter.UseAdvanced, "" );
						if( _adapter.UseAdvanced )
						{
							EditorGUI.indentLevel++;			
							_adapter.InfluenceDamage = ICEEditorLayout.DefaultSlider( "Damage", "", _adapter.InfluenceDamage, 0.05f, -100, 100, 0 );		
							_adapter.InfluenceStress = ICEEditorLayout.DefaultSlider( "Stress", "", _adapter.InfluenceStress, 0.05f, -100, 100, 0 );
							_adapter.InfluenceDebility = ICEEditorLayout.DefaultSlider( "Debility", "", _adapter.InfluenceDebility, 0.05f, -100, 100, 0 );
							_adapter.InfluenceHunger = ICEEditorLayout.DefaultSlider( "Hunger", "", _adapter.InfluenceHunger, 0.05f, -100, 100, 0 );			
							_adapter.InfluenceThirst = ICEEditorLayout.DefaultSlider( "Thirst", "", _adapter.InfluenceThirst, 0.05f, -100, 100, 0 );				
							EditorGUI.indentLevel--;
						}
					}
				EditorGUI.indentLevel--;
				EditorGUILayout.Separator();
			}



			_adapter.UsePlayerDamage = ICEEditorLayout.ToggleLeft( "Player Damage", "", _adapter.UsePlayerDamage, true );
			if( _adapter.UsePlayerDamage )
			{
				EditorGUI.indentLevel++;

				_adapter.UseMultiplePlayerDamageHandler = ICEEditorLayout.Toggle( "Use Multiple Damage Handler", "", _adapter.UseMultiplePlayerDamageHandler, "" );
				if( _adapter.UseMultiplePlayerDamageHandler )
				{
					foreach( ICECreatureRFPSPPlayerDamageObject _damage in _adapter.PlayerDamages )
					{
						ICEEditorLayout.BeginHorizontal();
							ICEEditorLayout.Label( _damage.DamageBehaviourModeKey , true );
							GUILayout.FlexibleSpace();
							if( GUILayout.Button( new GUIContent( "X", "Delete" ), ICEEditorStyle.CMDButton ) )
							{
								_adapter.PlayerDamages.Remove( _damage );
								return;
							}
						ICEEditorLayout.EndHorizontal();

						DrawDamage( _control, _damage );
					}

					ICEEditorStyle.SplitterByIndent(EditorGUI.indentLevel + 1);
					ICEEditorLayout.BeginHorizontal();
					_damage_behaviour = EditorBehaviour.BehaviourPopup( _control, "Add Damage Handler by Behaviour", "", _damage_behaviour );

					EditorGUI.BeginDisabledGroup( _damage_behaviour.Trim() == "" );
						if( GUILayout.Button( new GUIContent( "ADD", "Adds a new damage handler" ), ICEEditorStyle.CMDButtonDouble ) )
							_adapter.AddPlayerDamage( _damage_behaviour );
					EditorGUI.EndDisabledGroup();
					ICEEditorLayout.EndHorizontal();
				}
				else
				{
					DrawDamage( _control, _adapter.SimpleDamage );
				}

				EditorGUI.indentLevel--;
			}

			EditorGUI.indentLevel++;
			EditorGUILayout.Separator();


		}

		private void DrawDamage( ICECreatureControl _control, ICECreatureRFPSPPlayerDamageObject _damage )
		{
			EditorGUI.indentLevel++;
			_damage.Damage = ICEEditorLayout.Slider( "Damage", "", _damage.Damage, 0.05f, 0, 100 );
			_damage.Type =  (DamageType)ICEEditorLayout.EnumPopup( "Damage Type", "", _damage.Type );
			EditorGUILayout.Separator();
			_damage.DamageBehaviourModeKey = EditorBehaviour.BehaviourSelect( _control, "Trigger Behaviour", "", _damage.DamageBehaviourModeKey , "ATTACK" );
			
			_damage.Range = ICEEditorLayout.MaxDefaultSlider( "Trigger Range", "", _damage.Range, 0.05f, 0, ref _damage.RangeMax, 2, ""  );
			ICEEditorLayout.MinMaxGroup( "Trigger Interval", "", ref _damage.IntervalMin, ref _damage.IntervalMax, 0,60, 0.05f, "" );
			ICEEditorLayout.MinMaxGroup( "Trigger Interruption Interval", "", ref _damage.InterruptionIntervalMin, ref _damage.InterruptionIntervalMax, 0,60, 0.05f, "" );
			ICEEditorLayout.MinMaxGroup( "Trigger Limiter", "", ref _damage.LimitMin, ref _damage.LimitMax, 0,60, 1, "" );
			_damage.Force = ICEEditorLayout.DefaultSlider( "Force", "", _damage.Force, 0.25f, 0, 100, 20.0f, "" );
			_damage.MuzzleFlash = (Renderer)EditorGUILayout.ObjectField( "Muzzle Flash", _damage.MuzzleFlash, typeof(Renderer), true );
			_damage.Sound = (AudioClip)EditorGUILayout.ObjectField( "Fire Sound", _damage.Sound, typeof(AudioClip), false );
			_damage.FxRandonPitch = ICEEditorLayout.DefaultSlider( "Fire Fx Randon Pitch", "", _damage.FxRandonPitch, 0.001f, -1, 1, 0.86f, "" );
			EditorGUI.indentLevel--;
		}
	}
}