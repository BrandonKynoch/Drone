//
//  DataHandler.swift
//  NN Analysis
//
//  Created by Brandon Kynoch on 7/13/22.
//

import Foundation

class DataHandler: ObservableObject {
    // SINGLETON PATTERN ///////////////////////////////////
    private static var privateSingleton: DataHandler?
    public static var singleton: DataHandler = {
        if privateSingleton == nil {
            privateSingleton = DataHandler()
        }
        return privateSingleton!
    }()
    // SINGLETON PATTERN ///////////////////////////////////
    
    
    @Published private(set) var openTrainingFolders = [URL]()
    
    init() {
        guard DataHandler.privateSingleton == nil else {
            print("\nError: Attempted to initialize duplicate data handler\n")
            return
        }
        DataHandler.privateSingleton = self
    }
    
    public func openTrainingFolder(fromPath path: URL) {
        if path.isDirectory == true {
            // path is a directory
            openTrainingFolders.append(path)
        } else {
            // path is a file
            let enclosingFolder = path.deletingLastPathComponent()
            if !openTrainingFolders.contains(enclosingFolder) {
                openTrainingFolders.append(enclosingFolder)
            }
        }
    }
}
