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
    
    @Published private(set) var epochFolders = [EpochFolder]()
    private var unknownEpochFolder: EpochFolder! = nil
    
    @Published private(set) var openNetworkEntityFolders = [NNGroupFolder]() // Each folder consists of several NNs forming a single combined NN
    
    @Published private(set) var currentViewingEpochFolder: EpochFolder? = nil
    @Published public var currentViewingTrainingFolder: NNGroupFolder? = nil {
        didSet {
            currentViewingEpochFolder = currentViewingTrainingFolder?.epochFolder ?? nil
        }
    }
    
    
    
    init() {
        guard DataHandler.privateSingleton == nil else {
            print("\nError: Attempted to initialize duplicate data handler\n")
            return
        }
        
        if let resourceURL = Bundle.main.resourceURL {
            unknownEpochFolder = EpochFolder(folder: resourceURL.appendingPathComponent("Unknown Epochs"))
            epochFolders.append(unknownEpochFolder)
        }
        
        DataHandler.privateSingleton = self
    }
    
    public func openTrainingFolder(fromPath path: URL, epochFolder: EpochFolder? = nil) {
        withAnimation {
            var correctPath: URL
            if path.isDirectory == true {
                // path is a directory
                correctPath = path
            } else {
                // path is a file
                correctPath = path.deletingLastPathComponent()
            }
            
            let nnsInFolder = getAllFilesInDirectory(directory: correctPath, extensionWanted: "NN")
            
            if nnsInFolder.names.count > 0 {
                // Load NN entity
                if !openNetworkEntityFolders.contains(NNGroupFolder(folder: correctPath)) {
                    let nngFolder = NNGroupFolder(folder: correctPath, epochFolder: epochFolder)
                    nngFolder.FetchNNGroup()
                    
                    if let nng = nngFolder.nng, nng.loadedSuccessfully {
                        openNetworkEntityFolders.append(nngFolder)
                        currentViewingTrainingFolder = nngFolder
                        
                        if let epochFolder = epochFolder {
                            epochFolder.RegisterNNForEpoch(nn: nngFolder)
                        } else {
                            unknownEpochFolder.RegisterNNForEpoch(nn: nngFolder)
                            
                            // Sort by filename if we are not loading from epoch
                            NNGroupFolder.SortGroups(groups: &openNetworkEntityFolders)
                        }
                    }
                }
            } else {
                // Potentially an epoch folder -> try load NNs in subdirectory
                let epochFolder = EpochFolder(folder: correctPath)
                epochFolders.append(epochFolder)
                
                let subDirs = getAllFilesInDirectory(directory: correctPath, extensionWanted: nil)
                
                if PrefsHandler.singleton.loadAllDrones {
                    for dir in subDirs.paths {
                        openTrainingFolder(fromPath: dir, epochFolder: epochFolder)
                    }
                } else {
                    let targetDir = subDirs.paths.filter({ url in url.lastPathComponent == "0"})
                    if targetDir.count == 1 {
                        openTrainingFolder(fromPath: targetDir[0], epochFolder: epochFolder)
                    }
                }
                
                epochFolder.Sort()
                NNGroupFolder.SortGroups(groups: &openNetworkEntityFolders)
            }
        }
    }
    
    public func SetCurrentViewingTrainingFolder(path: URL) {
        for folder in openNetworkEntityFolders {
            if folder.folder == path {
                withAnimation {
                    currentViewingTrainingFolder = folder
                }
                break
            }
        }
    }
    
    public func CycleCurrentViewingEpochFolder(offset: Int) {
        // TODO: CONTINUE FROM HERE
        // Change current viewing trainign folder from keyboard input
    }
    
    public func CloseTrainingFolder(path: URL) {
        withAnimation {
            if let currentViewingTrainingFolder = currentViewingTrainingFolder {
                if currentViewingTrainingFolder.folder == path {
                    self.currentViewingTrainingFolder = nil
                }
            }
            
            openNetworkEntityFolders.removeAll(where: { folder in
                return folder.folder == path
            })
        }
    }
}
    
class EpochFolder: ObservableObject, Equatable, Hashable {
    private(set) var folder: URL
    private(set) var nns: [NNGroupFolder]
    
    @Published public var expandedView: Bool = false
    
    init(folder: URL) {
        self.folder = folder
        self.nns = [NNGroupFolder]()
    }
    
    public func RegisterNNForEpoch(nn: NNGroupFolder) {
        if !nns.contains(where: { n in n == nn }) {
            nns.append(nn)
        }
    }
    
    func Sort() {
        nns.sort(by: { l, r in
            return (Int(l.folder.lastPathComponent) ?? 0) > (Int(r.folder.lastPathComponent) ?? 0)
        })
    }
    
    static func == (lhs: EpochFolder, rhs: EpochFolder) -> Bool {
        return lhs.folder == rhs.folder
    }
    
    func hash(into hasher: inout Hasher) {
        hasher.combine(folder)
    }
}

class NNGroupFolder: Equatable, Hashable {
    private(set) var folder: URL
    public var nng: NNGroup?
    private(set) var epochFolder: EpochFolder?
    
    init(folder: URL, epochFolder: EpochFolder? = nil) {
        self.folder = folder
        self.nng = nil
        self.epochFolder = epochFolder
    }
    
    public func FetchNNGroup() {
        self.nng = NNGroup(folder: folder)
    }
    
    static func SortGroups(groups: inout [NNGroupFolder]) {
        groups.sort(by: { l, r in
            return (Int(l.folder.lastPathComponent) ?? 0) > (Int(r.folder.lastPathComponent) ?? 0)
        })
    }
    
    static func == (lhs: NNGroupFolder, rhs: NNGroupFolder) -> Bool {
        return lhs.folder == rhs.folder
    }
    
    func hash(into hasher: inout Hasher) {
        hasher.combine(folder)
    }
}
