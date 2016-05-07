// ##############################################################################
//
// ICECreatureRFPSPAdapter.cs
// Version 1.1.21
//
// © Pit Vetterick, ICE Technologies Consulting LTD. All Rights Reserved.
// http://www.ice-technologies.com
// mailto:support@ice-technologies.com
// 
// Unity Asset Store End User License Agreement (EULA)
// http://unity3d.com/legal/as_terms
//
//
// The ICECreatureRFPSPAdapter is derived from the original RFPSP CharacterDamage class and will override the 
// inherited ApplyDamage method, to receive and handle damages during the runtime. 
//
// This will provoke an error, because the original ApplyDamage method is a non-virtual member and can’t be 
// overwritten by derived classes. 
// 
//  8<---
// Assets/ICE/ICECreatureControl/Scripts/Adapter/RFPSP/ICECreatureRFPSPAdapter.cs(273,38): error CS0506: 
// `ICE.Creatures.Adapter.ICECreatureRFPSPAdapter.ApplyDamage(float, UnityEngine.Vector3, UnityEngine.Vector3, UnityEngine.Transform, bool)': 
// cannot override inherited member `CharacterDamage.ApplyDamage(float, UnityEngine.Vector3, UnityEngine.Vector3, UnityEngine.Transform, bool)' 
// because it is not marked virtual, abstract or override
// --->8
//
// To fix this issue you have simply to add the virtual modifier to the method declaration for the ApplyDamage function 
// in line 32 of the original CharacterDamage class.
//
// So simply open CharacterDamage.cs  (RFPSP -> Scripts -> AI -> CharacterDamage.cs) and change …
// public void ApplyDamage ( float damage, Vector3 attackDir, Vector3 attackerPos, Transform attacker, bool isPlayer ) 
// to ...
// public virtual void ApplyDamage ( float damage, Vector3 attackDir, Vector3 attackerPos, Transform attacker, bool isPlayer )
// that’s all :)
//
// ##############################################################################



using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using ICE.Creatures.Components;

namespace ICE.Creatures.Adapter
{
	public enum DamageType{
		UNDEFINED,
		NPC,
		DAMAGEZONE,
		EXPLOSIVE,
		MINE,
		WEAPON
	}

	public struct DamageInfo{
		public DamageType Type;
		public float Damage;
	}

	[System.Serializable]
	public class ICECreatureRFPSPCreatureDamageObject : System.Object 
	{
		public ICECreatureRFPSPCreatureDamageObject(){}
		public ICECreatureRFPSPCreatureDamageObject( DamageType _damage_type )
		{
			Type = _damage_type;
		}

		public bool UseAdvanced = false;
		public float InfluenceDamage = 1.5f;
		public float InfluenceStress = 0.5f;
		public float InfluenceDebility = 0.5f;
		public float InfluenceHunger = 0;
		public float InfluenceThirst = 0;
		public string BehaviourModeKey = "";
		public DamageType Type = DamageType.UNDEFINED;
	}

	[System.Serializable]
	public class ICECreatureRFPSPPlayerDamageObject : System.Object 
	{
		public ICECreatureRFPSPPlayerDamageObject(){}
		public ICECreatureRFPSPPlayerDamageObject( string _behaviour )
		{
			DamageBehaviourModeKey = _behaviour;
		}

		public string DamageBehaviourModeKey = "";
		public float Damage = 1;
		public float RangeMax = 100;
		public float Range = 2;
		public float IntervalMin = 0.09f;
		public float IntervalMax = 0.09f;
		public float LimitMin = 3;
		public float LimitMax = 6;
		public float InterruptionIntervalMin = 2f;
		public float InterruptionIntervalMax = 6f;
		public DamageType Type = DamageType.UNDEFINED;
		public float Force = 20.0f;			
		public Renderer MuzzleFlash;		
		public AudioClip Sound;
		public float FxRandonPitch = 0.86f;

		private float m_PlayerDamageEffectTimer = 0;
		public float PlayerDamageEffectTime = 0.3f;

		private float m_PlayerDamageTimer = 0;
		private float m_PlayerDamageInterval = 0.09f;
		private int m_PlayerDamageCounter = 0;
		private int m_PlayerDamageLimit = 3;
		private float m_PlayerDamageInterruptionTimer = 0;
		private float m_PlayerDamageInterruptionTime = 0.5f;

		public void Init ()
		{
			m_PlayerDamageInterval = 0;//Random.Range( IntervalMin, IntervalMax );
			m_PlayerDamageInterruptionTime = Random.Range( InterruptionIntervalMin, InterruptionIntervalMax );
			m_PlayerDamageLimit = (int)Random.Range( LimitMin, LimitMax );

			if( MuzzleFlash )
				MuzzleFlash.enabled = false;
		}

		public void Reset ()
		{
			m_PlayerDamageInterval = Random.Range( IntervalMin, IntervalMax );
			m_PlayerDamageInterruptionTime = Random.Range( InterruptionIntervalMin, InterruptionIntervalMax );
			m_PlayerDamageLimit = (int)Random.Range( LimitMin, LimitMax );
			
			if( MuzzleFlash )
				MuzzleFlash.enabled = false;
		}

		public void Update ()
		{
			if( MuzzleFlash )
			{
				m_PlayerDamageEffectTimer += Time.deltaTime;
				if( m_PlayerDamageEffectTimer > PlayerDamageEffectTime )
					MuzzleFlash.enabled = false;
			}
		}

		public bool IsActive( ICECreatureControl _control )
		{
			if( DamageBehaviourModeKey != "" && 
			   _control.Creature.Behaviour.BehaviourModeKey == DamageBehaviourModeKey &&
			   _control.Creature.ActiveTargetMovePositionDistance <= Range )
				return true;
			else
				return false;
		}

		public bool IsReady( ICECreatureControl _control )
		{
			if( _control == null || _control.Creature == null || _control.Creature.Behaviour == null )
				return false;

			bool _ready = false;

			if( IsActive( _control ) )
			{
				if( m_PlayerDamageCounter < m_PlayerDamageLimit || Mathf.Max( LimitMin, LimitMax ) == 0 )
				{
					m_PlayerDamageTimer += Time.deltaTime;
					if( m_PlayerDamageTimer >= m_PlayerDamageInterval )
					{
						_ready = true;
						m_PlayerDamageCounter += 1;
						m_PlayerDamageTimer = 0;
						m_PlayerDamageInterval = Random.Range( IntervalMin, IntervalMax );
						
						if( MuzzleFlash )
						{
							m_PlayerDamageEffectTimer = 0;
							PlayerDamageEffectTime = m_PlayerDamageInterval / 3;
							MuzzleFlash.enabled = true;
						}
					}
					
					m_PlayerDamageInterruptionTime = Random.Range( InterruptionIntervalMin, InterruptionIntervalMax );
				}
				else
				{
					m_PlayerDamageInterruptionTimer += Time.deltaTime; 
					if( m_PlayerDamageInterruptionTimer > m_PlayerDamageInterruptionTime || Mathf.Max( InterruptionIntervalMin, InterruptionIntervalMax ) == 0 )
					{
						m_PlayerDamageCounter = 0;
						m_PlayerDamageInterruptionTimer = 0;
						m_PlayerDamageLimit = (int)Random.Range( LimitMin, LimitMax );
					}
				}
		
			}
			else
				Reset();
			
			return _ready;
		}
	}

	[RequireComponent (typeof (ICECreatureControl))]
	public class ICECreatureRFPSPAdapter : CharacterDamage 
	{

		private ICECreatureControl m_Controller = null;

		public List<ICECreatureRFPSPCreatureDamageObject> CreatureDamages = new List<ICECreatureRFPSPCreatureDamageObject>();
		public List<ICECreatureRFPSPPlayerDamageObject> PlayerDamages = new List<ICECreatureRFPSPPlayerDamageObject>();

		public bool UseCreatureDamage = true;
		public bool UseMultipleCreatureDamageHandler = false;
		public bool UseAdvanced = false;
		public float InfluenceDamage = 1.5f;
		public float InfluenceStress = 0.5f;
		public float InfluenceDebility = 0.5f;
		public float InfluenceHunger = 0;
		public float InfluenceThirst = 0;
		public string BehaviourModeKey = "";

		public bool UseMultiplePlayerDamageHandler = false;
		public bool UsePlayerDamage = false;

		private LayerMask m_SearchMask = 0;
		private ICECreatureRFPSPPlayerDamageObject m_ActiveDamage = null;
		public ICECreatureRFPSPPlayerDamageObject SimpleDamage = new ICECreatureRFPSPPlayerDamageObject();
		/*
		protected new void Awake ()
		{

			///m_Audio = GetComponent<AudioSource>();
			
			//CurrentHealth = MaxHealth;
			
			// check for obsolete respawn-related parameters, create a vp_Respawner
			// component (if necessary) and disable such values on this component
			// NOTE: this check is temporary and will be removed in the future
			//CheckForObsoleteParams();
			
			//Instances.Add(GetComponent<Collider>(), this);
		}

		protected new void OnEnable()
		{
			m_Controller = GetComponent<ICECreatureControl>();
			
			//m_Player = GameObject.FindGameObjectWithTag("Player");
			//m_PlayerDamageHandler = m_Player.transform.GetComponent<CharacterDamage>();



			m_SearchMask = ~(~(1 << 10) & ~(1 << 19) & ~(1 << 13) & ~(1 << 11) & ~(1 << 20));
		}
*/
		public bool AddCreatureDamage( DamageType _damage_type )
		{
			ICECreatureRFPSPCreatureDamageObject _damage = GetCreatureDamage( _damage_type );

			if( _damage != null )
				return false;

			CreatureDamages.Add( new ICECreatureRFPSPCreatureDamageObject( _damage_type ) );
			return true;
		}

		private ICECreatureRFPSPCreatureDamageObject GetCreatureDamage( DamageType _damage_type )
		{
			foreach( ICECreatureRFPSPCreatureDamageObject _damage in CreatureDamages ){
				if( _damage != null && _damage.Type == _damage_type )
					return _damage;
			}
			return null;
		}

		public override void ApplyDamage( float _damage_value, Vector3 _attack_direction, Vector3 _attacker_position, Transform _attacker, bool _is_player, bool _is_explosion, Rigidbody _rigidbody = null, float _force = 0f  )
		{
			if( ! UseCreatureDamage )
				return;
			
			m_Controller = GetComponent<ICECreatureControl>();			
			if( m_Controller != null )
			{
				if( CreatureDamages.Count > 0 )
				{
					DamageType _damage_type = DamageType.WEAPON;
					ICECreatureRFPSPCreatureDamageObject _damage = GetCreatureDamage( _damage_type );
					if( _damage != null )
					{
						if( _damage.UseAdvanced )
						{
							m_Controller.Creature.Status.AddDamage( _damage.InfluenceDamage );
							m_Controller.Creature.Status.AddStress( _damage.InfluenceStress );
							m_Controller.Creature.Status.AddDebility( _damage.InfluenceDebility );
							m_Controller.Creature.Status.AddHunger( _damage.InfluenceHunger );
							m_Controller.Creature.Status.AddThirst( _damage.InfluenceThirst );
						}
						else
						{
							m_Controller.Creature.Status.AddDamage( _damage_value );
						}
						
						if( _damage.BehaviourModeKey != "" && m_Controller.Creature.Status.IsDead == false )
							m_Controller.Creature.Behaviour.SetBehaviourModeByKey( _damage.BehaviourModeKey );
					}
				}
				else
				{
					if( UseAdvanced )
					{
						m_Controller.Creature.Status.AddDamage( InfluenceDamage );
						m_Controller.Creature.Status.AddStress( InfluenceStress );
						m_Controller.Creature.Status.AddDebility( InfluenceDebility );
						m_Controller.Creature.Status.AddHunger( InfluenceHunger );
						m_Controller.Creature.Status.AddThirst( InfluenceThirst );
					}
					else
					{
						m_Controller.Creature.Status.AddDamage( _damage_value );
					}
					
					if( BehaviourModeKey != "" && m_Controller.Creature.Status.IsDead == false )
						m_Controller.Creature.Behaviour.SetBehaviourModeByKey( BehaviourModeKey );
				}
			}
		}

		public bool AddPlayerDamage( string _behaviour )
		{
			if( _behaviour.Trim() == "" )
				return false;

			PlayerDamages.Add( new ICECreatureRFPSPPlayerDamageObject( _behaviour ) );
			return true;
		}

		private ICECreatureRFPSPPlayerDamageObject GetPlayerDamage()
		{
			m_Controller = GetComponent<ICECreatureControl>();

			if( PlayerDamages.Count > 0 )
			{
				List<ICECreatureRFPSPPlayerDamageObject> _damages = new List<ICECreatureRFPSPPlayerDamageObject>();
				foreach( ICECreatureRFPSPPlayerDamageObject _damage in PlayerDamages )
				{
					if( _damage.IsActive( m_Controller ) )
						_damages.Add( _damage );
				}

				if( _damages.Count == 1 ){
					return _damages[0];
				}else if( _damages.Count > 1 ){
					return _damages[Random.Range(0,_damages.Count)];
				}else
					return null;
			}
			else if( SimpleDamage.IsActive( m_Controller ) )
				return SimpleDamage;
			else
				return null;
		}



		public void Update ()
		{
			if( ! UsePlayerDamage )
				return;

			ICECreatureRFPSPPlayerDamageObject _damage = GetPlayerDamage();
			
			if( _damage != null )
			{
				if( m_ActiveDamage != _damage )
				{
					m_ActiveDamage = _damage;
					m_ActiveDamage.Init();
				}
			}
			else if( m_ActiveDamage != null )
			{
				m_ActiveDamage.Reset();				
				m_ActiveDamage = null;
			}

			if( m_ActiveDamage != null )
			{
				if( m_ActiveDamage.IsReady( m_Controller ) )
					FireOneShot( m_ActiveDamage );

				m_ActiveDamage.Update();
			}


		}



		public void LateUpdate (){

		}

		public void FireOneShot ( ICECreatureRFPSPPlayerDamageObject _damage ){
			
			if( m_Controller == null || m_Controller.Creature == null || m_Controller.Creature == null )
				return;
			
			RaycastHit hit;
			/*
			if(m_Controller.TargetAIComponent){
				if(Vector3.Distance(m_Transform.position, m_Controller.lastVisibleTargetPosition) > 2.5f){
					targetPos = new Vector3(m_Controller.lastVisibleTargetPosition.x + Random.Range(-inaccuracy, inaccuracy), 
					                        m_Controller.lastVisibleTargetPosition.y + Random.Range(-inaccuracy, inaccuracy), 
					                        m_Controller.lastVisibleTargetPosition.z + Random.Range(-inaccuracy, inaccuracy));
				}else{
					targetPos = new Vector3(m_Controller.lastVisibleTargetPosition.x, 
					                        m_Controller.lastVisibleTargetPosition.y, 
					                        m_Controller.lastVisibleTargetPosition.z);
				}
			}else{
				
				if(m_FPSWalker.crouched || m_FPSWalker.prone){
					targetPos = m_Controller.lastVisibleTargetPosition;
				}else{
					targetPos = m_Controller.lastVisibleTargetPosition + (m_Controller.playerObj.transform.up * m_Controller.targetEyeHeight);
				}
				
			}*/
			
			Vector3 targetPos = m_Controller.Creature.ActiveTarget.TargetPosition;
			targetPos.y += 1;

			m_SearchMask = ~(~(1 << 10) & ~(1 << 19) & ~(1 << 13) & ~(1 << 11) & ~(1 << 20));
			
			Vector3 rayOrigin = new Vector3( transform.position.x, transform.position.y + m_Controller.Creature.Move.VisualSensorPosition.y, transform.position.z);
			Vector3 targetDir = (targetPos - rayOrigin).normalized;
			// Did we hit anything?
			if( Physics.Raycast(rayOrigin, targetDir, out hit, _damage.Range, m_SearchMask )) 
			{
				// Apply a force to the rigidbody we hit
				if (hit.rigidbody){
					hit.rigidbody.AddForceAtPosition( _damage.Force * targetDir / (Time.fixedDeltaTime * 100.0f), hit.point);
				}
				//draw impact effects where the weapon hit
				if (hit.collider.gameObject.layer != 11 
				    && hit.collider.gameObject.layer != 20){
					
					//if( m_WeaponEffectsComponent != null )
					//	m_WeaponEffectsComponent.ImpactEffects(hit, true);
				}
				

				//call the ApplyDamage() function in the script of the object hit TODO: can the GetComponents be replaced by direct calls using object identification by layer?
				switch(hit.collider.gameObject.layer){
				case 13://hit object is an NPC
					if(hit.collider.gameObject.GetComponent<CharacterDamage>()){
						hit.collider.gameObject.GetComponent<CharacterDamage>().ApplyDamage( _damage.Damage, Vector3.zero, transform.position, transform, false, false);
					}
					
					if(hit.collider.gameObject.GetComponent<ICECreatureRFPSPAdapter>()){
						hit.collider.gameObject.GetComponent<ICECreatureRFPSPAdapter>().ApplyDamage(_damage.Damage, Vector3.zero, transform.position, transform, false, false);
					}
					break;
				case 9://hit object is an apple
					if(hit.collider.gameObject.GetComponent<AppleFall>()){
						hit.collider.gameObject.GetComponent<AppleFall>().ApplyDamage(_damage.Damage);
					}	
					break;
				case 19://hit object is a breakable or explosive object
					if(hit.collider.gameObject.GetComponent<BreakableObject>()){
						hit.collider.gameObject.GetComponent<BreakableObject>().ApplyDamage(_damage.Damage);
					}else if(hit.collider.gameObject.GetComponent<ExplosiveObject>()){
						hit.collider.gameObject.GetComponent<ExplosiveObject>().ApplyDamage(_damage.Damage);
					}else if(hit.collider.gameObject.GetComponent<MineExplosion>()){
						hit.collider.gameObject.GetComponent<MineExplosion>().ApplyDamage(_damage.Damage);
					}
					break;
				case 11://hit object is player
					if(hit.collider.gameObject.GetComponent<FPSPlayer>()){
						hit.collider.gameObject.GetComponent<FPSPlayer>().ApplyDamage(_damage.Damage);
					}	
					if(hit.collider.gameObject.GetComponent<LeanColliderDamage>()){
						hit.collider.gameObject.GetComponent<LeanColliderDamage>().ApplyDamage(_damage.Damage);
					}	
					break;
				case 20://hit object is player lean collider
					if(hit.collider.gameObject.GetComponent<FPSPlayer>()){
						hit.collider.gameObject.GetComponent<FPSPlayer>().ApplyDamage(_damage.Damage);
					}	
					if(hit.collider.gameObject.GetComponent<LeanColliderDamage>()){
						hit.collider.gameObject.GetComponent<LeanColliderDamage>().ApplyDamage(_damage.Damage);
					}	
					break;
				default:
					break;	
				}
				
			}

			transform.gameObject.GetComponent<AudioSource>().clip = _damage.Sound;
			transform.gameObject.GetComponent<AudioSource>().pitch = Random.Range( _damage.FxRandonPitch, 1);
			transform.gameObject.GetComponent<AudioSource>().PlayOneShot(transform.gameObject.GetComponent<AudioSource>().clip, 0.9f / transform.gameObject.GetComponent<AudioSource>().volume);
			
		}
	}
}
