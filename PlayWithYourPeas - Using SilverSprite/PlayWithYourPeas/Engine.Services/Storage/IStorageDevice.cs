﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PlayWithYourPeas.Engine.Services.Storage
{
	/// <summary>
	/// Defines the interface for an object that can perform file operations.
	/// </summary>
	public interface IStorageDevice
	{
		/// <summary>
		/// Gets whether or not the device is in a state to receive file operation method calls.
		/// </summary>
		bool IsReady { get; }

		/// <summary>
		/// Saves a file.
		/// </summary>
		/// <param name="containerName">The name of the container in which to save the file.</param>
		/// <param name="fileName">The file to save.</param>
		/// <param name="saveAction">The save action to perform.</param>
		void Save(String containerName, String fileName, FileAction saveAction);

		/// <summary>
		/// Loads a file.
		/// </summary>
		/// <param name="containerName">The name of the container from which to load the file.</param>
		/// <param name="fileName">The file to load.</param>
		/// <param name="loadAction">The load action to perform.</param>
        void Load(String containerName, String fileName, FileAction loadAction);

		/// <summary>
		/// Deletes a file.
		/// </summary>
		/// <param name="containerName">The name of the container from which to delete the file.</param>
		/// <param name="fileName">The file to delete.</param>
        void Delete(String containerName, String fileName);

		/// <summary>
		/// Determines if a given file exists.
		/// </summary>
		/// <param name="containerName">The name of the container in which to check for the file.</param>
		/// <param name="fileName">The name of the file.</param>
		/// <returns>True if the file exists, false otherwise.</returns>
        bool FileExists(String containerName, String fileName);

		/// <summary>
		/// Gets an array of all files available in a container.
		/// </summary>
		/// <param name="containerName">The name of the container in which to search for files.</param>
		/// <returns>An array of file names of the files in the container.</returns>
        String[] GetFiles(String containerName);
               
		/// <summary>
		/// Gets an array of all files available in a container that match the given pattern
		/// </summary>
		/// <param name="containerName">The name of the container in which to search for files.</param>
		/// <param name="pattern">A search pattern to use to find files.</param>
		/// <returns>An array of file names of the files in the container that match the pattern.</returns>
        String[] GetFiles(String containerName, String pattern);
    }
}
