﻿using System;
using System.Collections.Generic;
using System.Collections;


namespace Nez
{
	public class ComponentList : IEnumerable<Component>, IEnumerable
	{
		Entity _entity;

		/// <summary>
		/// list of components added to the entity
		/// </summary>
		List<Component> _components = new List<Component>();

		/// <summary>
		/// The list of components that were added this frame. Used to group the components so we can process them simultaneously
		/// </summary>
		List<Component> _componentsToAdd = new List<Component>();

		/// <summary>
		/// The list of components that were marked for removal this frame. Used to group the components so we can process them simultaneously
		/// </summary>
		List<Component> _componentsToRemove = new List<Component>();


		public ComponentList( Entity entity )
		{
			this._entity = entity;
		}


		public void add( Component component )
		{
			_componentsToAdd.Add( component );
		}


		public void remove( Component component )
		{
			_componentsToRemove.Add( component );
		}


		/// <summary>
		/// removes all components from the component list
		/// </summary>
		public void removeAllComponents()
		{
			for( var i = 0; i < _components.Count; i++ )
			{
				// deal with renderLayer list if necessary
				if( _components[i] is RenderableComponent )
					_entity.scene.renderableComponents.remove( _components[i] as RenderableComponent );
				
				_components[i].onRemovedFromEntity();
				_components[i].entity = null;
			}
		}


		public void updateLists()
		{
			// handle removals
			if( _componentsToRemove.Count > 0 )
			{
				for( var i = 0; i < _componentsToRemove.Count; i++ )
				{
					var component = _componentsToRemove[i];

					// deal with renderLayer list if necessary
					if( component is RenderableComponent )
						_entity.scene.renderableComponents.remove( component as RenderableComponent );

					_components.Remove( component );
					component.onRemovedFromEntity();
					component.entity = null;
				}
				_componentsToRemove.Clear();
			}

			// handle additions
			if( _componentsToAdd.Count > 0 )
			{
				for( var i = 0; i < _componentsToAdd.Count; i++ )
				{
					var component = _componentsToAdd[i];

					if( component is RenderableComponent )
						_entity.scene.renderableComponents.add( component as RenderableComponent );

					_components.Add( component );
					component.onAddedToEntity();
				}

				// now that all components are added to the scene, we loop through again and call onAwake/onEnabled
				for( var i = 0; i < _componentsToAdd.Count; i++ )
				{
					var component = _componentsToAdd[i];
					component.onAwake();

					if( _entity.enabled && component.enabled )
						component.onEnabled();
				}

				_componentsToAdd.Clear();
			}
		}


		public int Count
		{
			get
			{
				return _components.Count;
			}
		}


		/// <summary>
		/// Gets the first component of type T and returns it. If no components are found returns null
		/// </summary>
		/// <returns>The component.</returns>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public T getComponent<T>() where T : Component
		{
			for( var i = 0; i < _components.Count; i++ )
			{
				var component = _components[i];
				if( component is T )
					return component as T;
			}

			// we also check the pending components just in case addComponent and getComponent are called in the same frame
			for( var i = 0; i < _componentsToAdd.Count; i++ )
			{
				var component = _componentsToAdd[i];
				if( component is T )
					return component as T;
			}

			return null;
		}


		#region IEnumerable and array access

		public Component this[int index]
		{
			get
			{
				return _components[index];
			}
		}


		public IEnumerator<Component> GetEnumerator()
		{
			return _components.GetEnumerator();
		}


		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return _components.GetEnumerator();
		}

		#endregion

	}
}
