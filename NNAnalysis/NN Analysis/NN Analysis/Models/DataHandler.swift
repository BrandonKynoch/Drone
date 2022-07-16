//
//  DataHandler.swift
//  NN Analysis
//
//  Created by Brandon Kynoch on 7/13/22.
//

import Foundation
import SwiftUI

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
    
    @Published private(set) var openTrainingFolders = [NNGroupFolder]()
    
    @Published public var currentViewingTrainingFolder: NNGroupFolder? = nil
    
    
    
    init() {
        guard DataHandler.privateSingleton == nil else {
            print("\nError: Attempted to initialize duplicate data handler\n")
            return
        }
        DataHandler.privateSingleton = self
    }
    
    public func openTrainingFolder(fromPath path: URL) {
        withAnimation {
            var correctPath: URL
            if path.isDirectory == true {
                // path is a directory
                correctPath = path
            } else {
                // path is a file
                correctPath = path.deletingLastPathComponent()
            }
            
            if !openTrainingFolders.contains(NNGroupFolder(folder: correctPath)) {
                let nngFolder = NNGroupFolder(folder: correctPath)
                nngFolder.FetchNNGroup()
                
                if let nng = nngFolder.nng, nng.loadedSuccessfully {
                    openTrainingFolders.append(nngFolder)
                    currentViewingTrainingFolder = nngFolder
                }
            }
        }
    }
    
    public func setCurrentViewingTrainingFolder(path: URL) {
        for folder in openTrainingFolders {
            if folder.folder == path {
                withAnimation {
                    currentViewingTrainingFolder = folder
                }
                break
            }
        }
    }
    
    public func closeTrainingFolder(path: URL) {
        withAnimation {
            if let currentViewingTrainingFolder = currentViewingTrainingFolder {
                if currentViewingTrainingFolder.folder == path {
                    self.currentViewingTrainingFolder = nil
                }
            }
            
            openTrainingFolders.removeAll(where: { folder in
                return folder.folder == path
            })
        }
    }
}

class NNGroupFolder: Equatable, Hashable {
    private(set) var folder: URL
    public var nng: NNGroup?
    
    init(folder: URL) {
        self.folder = folder
        self.nng = nil
    }
    
    public func FetchNNGroup() {
        self.nng = NNGroup(folder: folder)
    }
    
    static func == (lhs: NNGroupFolder, rhs: NNGroupFolder) -> Bool {
        return lhs.folder == rhs.folder
    }
    
    func hash(into hasher: inout Hasher) {
        hasher.combine(folder)
    }
}
