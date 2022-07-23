//
//  NNGroup.swift
//  Neural Analysis
//
//  Created by Brandon Kynoch on 7/14/22.
//

import Foundation

// Represents a single network consisting of multiple NN files
class NNGroup : ObservableObject {
    public let folder: URL
    private(set) var loadedSuccessfully: Bool
    private(set) var nns: [NN]
    private(set) var meta: NNMeta?
    private(set) var nnNames: [String]
    
    @Published public var currentViewingNN: NN? = nil
    
    init(folder: URL) {
        self.folder = folder
        self.nns = [NN]()
        self.nnNames = [String]()
        self.loadedSuccessfully = false
        self.meta = nil
        
        // Load NN meta file
        let nnMeta = getAllFilesInDirectory(directory: folder, extensionWanted: "NNM")
        if nnMeta.names.count > 0 {
            if nnMeta.names.count > 1 {
                print("Warning: multiple meta files found... only one will be used")
            }
            
            if let loadedMetaFile = DataHandler.singleton.allMetaFiles[nnMeta.paths[0]] {
                // Meta file has already been loaded
                self.meta = loadedMetaFile
            } else {
                // Load meta file
                do {
                    meta = try NNMeta(fromFile: nnMeta.paths[0])
                } catch {
                    print("Failed to load NN meta")
                }
            }
        }
    }
    
    public func LoadNNs() {
        var errorWhileLoading = false
        
        let nnFiles = getAllFilesInDirectory(directory: folder, extensionWanted: "NN")
        
        if nnFiles.paths.count > 0 {
            var tmpNNs = [NN]()
            var tmpNNNames = [String]()
            for nnPath in nnFiles.paths {
                do {
                    let nn = try NN(fromFile: nnPath)
                    tmpNNs.append(nn)
                    tmpNNNames.append(nnPath.lastPathComponent)
                } catch {
                    print("Failed to load NN")
                }
            }
            nns = tmpNNs
            nnNames = tmpNNNames
        } else {
            nns = [NN]()
            nnNames = [String]()
            errorWhileLoading = true
        }
        
        loadedSuccessfully = !errorWhileLoading
    }
    
    public func SetCurrentViewingNNFromName(fileName: String) {
        for nn in nns {
            if nn.name == fileName {
                currentViewingNN = nn
                break
            }
        }
    }
}