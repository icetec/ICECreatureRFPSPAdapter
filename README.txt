Dear customers,
additional to the ICECreatureRFPSPAdapter it is necessary to add the following code snippets to the CharacterDamage handler for layer 13 into following RFPSP scripts: 

•	NPCAttack
•	ExplosiveObject
•	MineExplosion
•	WeaponBehavior 

8<---
if(hit.collider.gameObject.GetComponent<ICE.Creatures.Adapter.ICECreatureRFPSPAdapter>()){
	hit.collider.gameObject.GetComponent<ICE.Creatures.Adapter.ICECreatureRFPSPAdapter>().ApplyDamage(damageAmt, Vector3.zero, myTransform.position, myTransform, false);
}
8<---

... and this snippet to layer 13 in the DamageZone script

8<---
ICE.Creatures.Adapter.ICECreatureRFPSPAdapter CC = col.GetComponent<ICE.Creatures.Adapter.ICECreatureRFPSPAdapter>();
if( CC != null && damageTime < Time.time){
	CC.ApplyDamage(damage, Vector3.zero, transform.position, null, false);
	damageTime = Time.time + delay;
}
8<---

I hope you will enjoy the work with ICE.

Have a nice day!
Yours, Pit

