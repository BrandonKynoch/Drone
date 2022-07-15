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
    
    init(folder: URL) {
        self.folder = folder
        var errorWhileLoading = false
        
        let nnFiles = getAllFilesInDirectory(directory: folder, extensionWanted: "NN")
        
        if nnFiles.paths.count > 0 {
            var tmpNNs = [NN]()
            for nn in nnFiles.paths {
                do {
                    let nn = try NN(fromFile: nn)
                    tmpNNs.append(nn)
                } catch {
                    print("Failed to create NN")
                }
            }
            nns = tmpNNs
        } else {
            nns = [NN]()
            errorWhileLoading = true
        }
        
        loadedSuccessfully = !errorWhileLoading
    }
}
