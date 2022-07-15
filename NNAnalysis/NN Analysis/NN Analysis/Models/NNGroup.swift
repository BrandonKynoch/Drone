//
//  NNGroup.swift
//  Neural Analysis
//
//  Created by Brandon Kynoch on 7/14/22.
//

import Foundation

// Represents a single network consisting of multiple NN files
class NNGroup : ObservableObject {
    let folder: URL
    let loadedSuccessfully: Bool
    let nns: [NN]
    let nnNames: [String]
    
    @Published public var currentViewingNN: NN? = nil
    
    init(folder: URL) {
        self.folder = folder
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
                    print("Failed to create NN")
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
