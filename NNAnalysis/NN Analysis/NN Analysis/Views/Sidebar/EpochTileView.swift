//
//  EpochTileView.swift
//  Neural Analysis
//
//  Created by Brandon Kynoch on 2022/07/17.
//

import SwiftUI

struct EpochTileView: View {
    let epoch: EpochFolder
    
    var body: some View {
        ZStack {
            PanelView(type: .withinWindow)
            
            VStack {
                Text(epoch.folder.lastPathComponent)
                    .foregroundColor(COL_ACC1)
                    .modifier(TitleTextModifier())
                
                ForEach (epoch.nns, id: \.folder) { folder in
                    VStack {
                        DirectoryTileView(targetDir: folder.folder)
                            .frame(height: 30)
                    }.frame(height: 50)
                }
            }
            .padding()
        }
    }
}

//struct EpochTileView_Previews: PreviewProvider {
//    static var previews: some View {
//        EpochTileView()
//    }
//}
